namespace TDKiosk
{
    public class Button
    {
        public Button(PageType link, string text, bool isActive = false)
        {
            Link = link;
            IsActive = isActive;
            Text = text;
        }

        public PageType Link { private set; get; }
        public string Text { private set; get; }
        public bool IsActive { private set; get; }
    }
}
