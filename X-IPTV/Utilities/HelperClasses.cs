namespace X_IPTV.Utilities
{
    public static class DialogHelpers
    {
        /// <summary>
        /// Opens a dialog for folder selection and returns the full path and folder name.
        /// </summary>
        /// <returns>A tuple containing the full path to the selected folder and the folder name only.</returns>
        public static (string FullPath, string FolderName) FolderDialogSelectFolder()
        {
            // Configure open folder dialog box
            Microsoft.Win32.OpenFolderDialog dialog = new();

            dialog.Multiselect = false;
            dialog.Title = "Select a folder";

            // Show open folder dialog box
            bool? result = dialog.ShowDialog();

            // Process open folder dialog box results
            if (result == true)
            {
                // Get the selected folder
                string fullPathToFolder = dialog.FolderName;
                string folderNameOnly = dialog.SafeFolderName;

                return (fullPathToFolder, folderNameOnly);
            }
            else
            {
                // Return empty strings or nulls, or handle as needed if the dialog is canceled
                return (null, null);
            }
        }

        /// <summary>
        /// Opens a dialog for file selection based on the provided filter and returns the selected file's full path.
        /// </summary>
        /// <param name="filter">The filter string that determines the types of files to be displayed by the dialog. 
        /// Example: "Image Files (*.png;*.jpg)|*.png;*.jpg|Text Files (*.txt)|*.txt"</param>
        /// <returns>A tuple containing the full path to the selected file and a boolean indicating whether a file was selected.</returns>
        public static (string FileFullPath, bool FileSelected) FileDialogSelectFile(string filter)
        {
            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = filter // Use the passed filter
            };

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // File was selected
                string filename = dialog.FileName;
                return (filename, true); // Return full file path and true for file selected
            }
            else
            {
                // Dialog was canceled, no file selected
                return (null, false); // Return null and false for file selected
            }
        }
    }
}
