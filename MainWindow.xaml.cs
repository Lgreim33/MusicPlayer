using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using Ookii.Dialogs.Wpf;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;


namespace MusicPlayer
{
    public partial class MainWindow : Window
    {
        private List<Playlist> playlists = new(); // Hold a list of all playlists being tracked
        private bool isPlaying = false; // Is a song playing?
        private static readonly string PlaylistFilePath = "Playlists.json"; // Store the Names of the playlists and their file paths here
        private List<string> currentTrackList = new(); // List of track paths in the currently selected playlist
        private int currentTrackIndex = 0; // Keeps track of what song is playing in the current playlist
        private DispatcherTimer progressTimer;  // Timer for updating progress bar
        private bool isSeeking = false;

        public MainWindow()
        {
            InitializeComponent();
            LoadPlaylists(); // Load the playlists from the json file
            PopulateListBox(); // Add the playlists to the listbox


            // Initialize the timer
            progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10) // Set update interval to 10ms
            };
            progressTimer.Tick += ProgressTimer_Tick;

            SliderProgress.AddHandler(Thumb.DragStartedEvent, new DragStartedEventHandler(Thumb_DragStarted));
            SliderProgress.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler(Thumb_DragCompleted));
        }

        // Button listener for adding a new playlist
        private void BtnAddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            // Open a file dialog to select a folder
            var dialog = new VistaFolderBrowserDialog
            {
                Description = "Select a folder to use as a playlist",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true,
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            };

            if (dialog.ShowDialog() == true)
            {
                string folderPath = dialog.SelectedPath;

                // Make sure the path is real and then add it to our playlists, then add it to the listbox
                if (!playlists.Exists(p => p.Filepath == folderPath))
                {
                    Playlist newPlaylist = new Playlist(folderPath);
                    playlists.Add(newPlaylist);
                    PlaylistListBox.Items.Add(newPlaylist);
                    SavePlaylists();
                }
                else
                {
                    MessageBox.Show("Playlist Already Exists");
                }
            }
        }
        // Button listener for pausing and playing the current track
        private void On_Play(object sender, RoutedEventArgs e)
        {
            // if the song is playing then pause the song and stop the progress bar
            if (isPlaying)
            {
                currentSong.Pause();
                isPlaying = false;
                BtnPlayPause.Content = "▶";
                progressTimer.Stop();
            }
            // Do the opposite
            else
            {
                BtnPlayPause.Content = "||";
                isPlaying = true;
                currentSong.Play();
                progressTimer.Start();
            }
        }

        // Listener for when we want to skip to the next song
        private void On_Next(object sender, RoutedEventArgs e)
        {
            // Stop the current song
            StopSong();

            // Increment index
            currentTrackIndex += 1;
            // If we hit the end, just wrap around to the start
            if(currentTrackIndex > currentTrackList.Count)
            {
                currentTrackIndex = 0;
            }
            // play the newly selected track
            PlayCurrentTrack();
        }
        // Listener for when we want to go back a song
        private void On_Last(object sender, RoutedEventArgs e)
        { 
            // Stop current song
            StopSong();
            // Decrement, do not wrap around, stop at the first song
            currentTrackIndex -= 1;
            if (currentTrackIndex < 0)
            {
                currentTrackIndex = 0;
            }
           // Play new track
            PlayCurrentTrack();

        }

        // Listens for a new playlist being selected, automatically plays when selecting a new playlist
        private void playlist_selection_changed(object sender, SelectionChangedEventArgs e)
        {

            if (PlaylistListBox.SelectedItem is Playlist selected)
            {
                currentTrackList = new List<string>(
                Directory.GetFiles(selected.Filepath, "*.mp3") // Filter for audio files
                );

                // Enable the button if the playlist has songs in it
                BtnPlayPause.IsEnabled = currentTrackList.Count > 0;
                BtnNext.IsEnabled = currentTrackList.Count > 0;
                BtnPrevious.IsEnabled = currentTrackList.Count > 0;

                // Get potential cover art
                string[] coverImagePaths = Directory.GetFiles(selected.Filepath, "*.jpg");

                if (coverImagePaths.Length > 0)
                {
                    try
                    {
                        CoverArt.Source = new BitmapImage(new Uri(coverImagePaths[0], UriKind.Absolute));
                    }
                    catch (Exception ex)
                    {
                        CoverArt.Source = null;
                        MessageBox.Show("Error Adding Cover Art");
                    }
                }
                // If there is nothing to grab, just dont include cover art
                else
                {
                    CoverArt.Source = null;
                }
                // Nake sure we only play the first song in the playlist
                currentTrackIndex = 0;

                if (currentTrackList.Count > 0)
                {
                    PlayCurrentTrack();
                }
            }

        }

        // Start the currently indexed track in the currently selected playlist
        private void PlayCurrentTrack()
        {
            if (currentTrackIndex >= 0 && currentTrackIndex < currentTrackList.Count)
            {
                StopSong(); // Ensure previous song is stopped
                BtnPlayPause.Content = "||";
                // Get the new song's source
                currentSong.Source = new Uri(currentTrackList[currentTrackIndex], UriKind.Absolute);

                // Reset position to the start
                //currentSong.Position = TimeSpan.Zero;

                // Play the song, start the timer
                currentSong.Play();
                isPlaying = true;
                progressTimer.Start();

                // Update the display to show what is playing
                string songPath = currentTrackList[currentTrackIndex];
                TextCurrentSongName.Text = System.IO.Path.GetFileNameWithoutExtension(songPath);
            }
        }

        // Upon a piece of media ending, play the next track, or reset the playlist if its the last track
        private void Element_MediaEnded(object sender, EventArgs e)
        {
            if (!isPlaying || isSeeking) return;

            currentTrackIndex++;
            if (currentTrackIndex < currentTrackList.Count)
            {

                PlayCurrentTrack();
            }
            else
            {
                currentTrackIndex = 0;
                PlayCurrentTrack();
            }
        }

        // Remove a selected playlist, reset elements to their default state
        private void RemovePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistListBox.SelectedItem is Playlist selected)
            {
                StopSong();
                playlists.Remove(selected);
                PlaylistListBox.Items.Remove(selected);
                BtnPlayPause.IsEnabled = false;
                BtnNext.IsEnabled = false;
                BtnPrevious.IsEnabled = false;
                CoverArt.Source = null;
                TextCurrentSongName.Text = "Now Playing...";
                SavePlaylists();
            }
        }
        // Copy absolute path of selected playlist
        private void CopyPath_Click(object sender, RoutedEventArgs e)
        {
            Playlist current = (Playlist)PlaylistListBox.SelectedItem;
            Clipboard.SetText(current.Filepath);
        }

        // Load the serialized playlists
        private void LoadPlaylists()
        {
            if (File.Exists(PlaylistFilePath))
            {
                string json = File.ReadAllText(PlaylistFilePath);
                playlists = JsonSerializer.Deserialize<List<Playlist>>(json) ?? new List<Playlist>();
            }
        }

        // Serialize the playlists and save them
        private void SavePlaylists()
        {
            string json = JsonSerializer.Serialize(playlists, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(PlaylistFilePath, json);
        }

        // Adds the saved playlists to the listbox on startup
        private void PopulateListBox()
        {
            PlaylistListBox.Items.Clear();
            foreach (var playlist in playlists)
            {
                PlaylistListBox.Items.Add(playlist);
            }
        }

        // Upon opening media, set the maximum value of the slider bar to be the maximum duration of the selected song
        private void Element_MediaOpened(object sender, EventArgs e)
        {
            SliderProgress.Maximum = currentSong.NaturalDuration.TimeSpan.TotalMilliseconds;
    }


        // Timer Tick event, updates the progress slider
        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (currentSong.NaturalDuration.HasTimeSpan && !isSeeking)
            {
                // Update position based on the song's cummulatiove position
                SliderProgress.Value = currentSong.Position.TotalMilliseconds;
            }
        }
        // Event for determining when the user has started position seeking
        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            isSeeking = true;
            progressTimer.Stop();
        }

        // Event for determining when user has stopped seeking, starts song from landed position
        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (!isPlaying)
            {
                isSeeking = false;
                return;
            }

            // Get current slider value
            int sliderValue = (int)SliderProgress.Value;
            TimeSpan newPosition = TimeSpan.FromMilliseconds(sliderValue);

            if (currentSong.NaturalDuration.HasTimeSpan)
            {
                // Clamp the new position's value
                TimeSpan maxDuration = currentSong.NaturalDuration.TimeSpan;
                if (newPosition >= maxDuration)
                {
                    newPosition = maxDuration - TimeSpan.FromMilliseconds(100);
                }
            }
            // Set new position
            currentSong.Position = newPosition;
            isSeeking = false;

            // Resume timer only after the new position has been set
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (isPlaying)
                {
                    progressTimer.Start();
                }
            }), DispatcherPriority.Background);
        }

        // Helper function to stop the current ssong
        private void StopSong()
        {
            currentSong.Stop();
            isPlaying = false;
            BtnPlayPause.Content = "▶";
            progressTimer.Stop();
        }
    }
}