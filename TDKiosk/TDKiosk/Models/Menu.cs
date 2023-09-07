namespace TDKiosk
{
    public class Menu
    {
        public Menu(Button leftButton, Button rightButton)
        {
            this.leftButton = leftButton;
            this.rightButton = rightButton;
        }

        public Button leftButton { private set; get; }
        public Button rightButton { private set; get; }
    }
}
