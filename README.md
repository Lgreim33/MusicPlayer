Packages used:
Ookii.Dialogs.Wpf (Used for folder selection)

Overview:
General Info:
Any folder in your filesystem can be a playlist. The program will read your selected playlist and play any mp3 files contained within the folder, and select any jpeg file as cover art. This keeps the UI simple, and is an intuitive way to customize playlists.

Adding a Playlist:
To get started, you’ll want to select the “Add playlist button,” from there you can select any folder you want, the program will not stop you from selecting a folder based on the contents. However, a dialog box will show up warning you if you select a folder with the same name as an existing playlist. Then, you can select the resulting list item in the list box. This will automatically begin playing the valid mp3 tracks, and will use one jpeg file from the folder as the cover art for the playlist. Selecting another playlist in the list box will automatically load and begin playing that playlist. 

Seeking:
The progress slider automatically updates itself as the song plays, resetting upon hitting the end of the song and playing the next. You can drag this slider to any position to play the song from the point in time relatively equal to the slider’s line position (i.e. moving the slider to the center will play the song from the middle onward).

Pausing, Skipping, Go Back:
To pause the track, simply hit the middle pause button, you cannot use the progress slider seek feature while paused, it will snap back to the original position upon playing. To go to the previous song hit the back button, and for the next song hit the next button. Hitting the next button on the last track in the playlist will start the initial song in the playlist. Hitting the last button on the first song in the playlist will simply restart the first song. It should be noted that upon playing a new song, the UI automatically updates to show the song name, and resets the progress slider.

Right Click Features:
For right click features, hover over the selected playlist and right click. Selecting “Delete” will delete the selected playlist. This will remove it from the listbox and stop the current song from playing. Select “Copy Path” to copy the filesystem path of the playlist.

Playlist End:
Upon the playlist ending it will not automatically restart, you must hit the “next” button to start the first song again.
