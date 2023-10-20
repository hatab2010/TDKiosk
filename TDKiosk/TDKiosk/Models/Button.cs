namespace TDKiosk
{
    public class Button
    {
        public Button(string text, bool isActive = false)
        {
            IsActive = isActive;
            Text = text;
        }

        public string Text { private set; get; }
        public bool IsActive { private set; get; }
    }
}
