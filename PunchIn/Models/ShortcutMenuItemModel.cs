using System.Collections.Generic;
using System.IO;
using System.Windows.Input;

namespace PunchIn.Models
{
    public class ShortcutMenuItemModel
    {
        public ShortcutMenuItemModel()
        {
            this.Children = new List<ShortcutMenuItemModel>();
        }
        public string Text { get; set; }
        public FileInfo File { get; set; }
        public string Icon { get; set; }
        public ICommand Command { get; set; }
        public IList<ShortcutMenuItemModel> Children { get; private set; }
    }
}
