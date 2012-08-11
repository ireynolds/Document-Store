using System;
using System.Windows.Navigation;

namespace NotepadTheNextVersion.Models
{
    /*
     * Defines an interface for interacting with elements in the
     * doc/dir listings page. 
     */
    public interface IActionable : IComparable<IActionable>
    {

        // Returns the path which locates this.
        Path Path { get; }

        // Returns true iff this IActionable is in your favorites
        bool IsFavorite { get;  set;}

        // Returns the name of this
        string Name { get; }

        bool IsTemp { get; set; }

        // Returns the name of this, without the trailing
        // ".txt" if applicable.
        string DisplayName { get; }

        // Navigates to a page that views this.
        void Open(NavigationService NavigationService);

        // Navigates to a page that moves this.
        void NavToMove(NavigationService NavigationService);

        // Moves this to the specified location. Returns an 
        // IActionable representing the new item.
        IActionable Move(Directory newLocation);

        // Navigates to a page that renames this. 
        void NavToRename(NavigationService NavigationService);

        // Renames this with the given filename. Returns an 
        // IActionable representing the new item.
        IActionable Rename(string newFileName);

        // Deletes this. Returns an IActionable representing 
        // the item if moved to trash, or null if deleted 
        // from trash.
        IActionable Delete();

        // If this is pinned to the start screen, unpins 
        // this. If this is not pinned, pins this. 
        void TogglePin();

        // Returns true iff the item this references
        // exists in the file system.
        bool Exists();

        // Returns a IActionable that points to the same location 
        // relative to a new root.
        IActionable SwapRoot();
    }
}
