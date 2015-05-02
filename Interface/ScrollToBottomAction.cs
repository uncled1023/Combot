using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Interface
{
    public class ScrollToBottomAction : TriggerAction<RichTextBox>
    {
        protected override void Invoke(object parameter)
        {
            AssociatedObject.ScrollToEnd();
        }
    }
}