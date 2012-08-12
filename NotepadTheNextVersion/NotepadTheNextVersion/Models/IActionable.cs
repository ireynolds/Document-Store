using System;
using System.Windows.Navigation;

namespace NotepadTheNextVersion.Models
{
    /// <summary>
    /// Defines an interface for interacting with items throughout the system.
    /// </summary>
    public interface IActionable : IComparable<IActionable>
    {

        /// <summary>
        /// Returns the path which locates this in IsolatedStorage
        /// </summary>
        Path Path { get; }

        /// <summary>
        /// Returns true iff this IActionable is in your favorites
        /// </summary>
        bool IsFavorite { get;  set;}

        /// <summary>
        /// Returns the filename of this 
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns true iff this is a temporary file (used for (a) opening to note editor
        /// and (b) between addnewitem and renameitem).
        /// </summary>
        bool IsTemp { get; set; }

        /// <summary>
        /// Navigates to a page to edit/view this.
        /// </summary>
        /// <param name="NavigationService"></param>
        void Open(NavigationService NavigationService);

        /// <summary>
        /// Navigates to a page that moves this.
        /// </summary>
        /// <param name="NavigationService"></param>
        void NavToMove(NavigationService NavigationService);

        /// <summary>
        /// Moves this to the specified location. Returns an IActionable representing 
        /// the new item. Does not rename the item.
        /// </summary>
        /// <param name="newLocation">The parent of the new location.</param>
        /// <returns></returns>
        IActionable Move(Directory newLocation);

        /// <summary>
        /// Navigates to a page that renames this. 
        /// </summary>
        /// <param name="NavigationService"></param>
        void NavToRename(NavigationService NavigationService);

        /// <summary>
        /// Renames this with the given filename. Returns an IActionable 
        /// representing the new item.
        /// </summary>
        /// <param name="newFileName"></param>
        /// <returns></returns>
        IActionable Rename(string newFileName);

        /// <summary>
        /// Deletes this. Returns an IActionable representing the item if moved to 
        /// trash, or null if deleted from trash.
        /// </summary>
        /// <returns></returns>
        IActionable Delete();

        /// <summary>
        /// If this is pinned to the start screen, unpins this. If this is not pinned, pins this.
        /// </summary>
        void TogglePin();

        /// <summary>
        /// Returns true iff the item this references exists in the file system.
        /// </summary>
        /// <returns></returns>
        bool Exists();

        /// <summary>
        /// Returns a IActionable that points to the same location relative to the current 
        /// user-set root directory.
        /// </summary>
        /// <returns></returns>
        IActionable SwapRoot();
    }
}
