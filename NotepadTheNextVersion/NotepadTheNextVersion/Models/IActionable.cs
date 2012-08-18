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
        PathStr Path { get; }

        /// <summary>
        /// Returns true iff this IActionable is in your favorites
        /// </summary>
        bool IsFavorite { get;  set;}

        /// <summary>
        /// Returns the filename of this 
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns true iff this IActionable is currently pinned to the start screen. 
        /// </summary>
        bool IsPinned { get; }

        /// <summary>
        /// Returns the filename of this without the trailing identifier
        /// </summary>
        string DisplayName { get; }

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
        /// If this is not in trash, moves to trash and returns the new item. If there is a file in trash with the
        /// same name, overwrites that file. If this is in trash, deletes from Isolated Storage and returns null. 
        /// </summary>
        /// <returns>
        /// If the item was moved to trash, returns the new item. If it was deleted from trash, return null.
        /// </returns>
        IActionable Delete(bool permanently = false);

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
