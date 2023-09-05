using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Forms;

namespace TDKiosk
{
    public static class Extensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field.GetCustomAttribute<DescriptionAttribute>();

            return attribute == null ? value.ToString() : attribute.Description;
        }

        public static T GetEnumValueFromDescription<T>(string description) where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                    if (attribute.Description == description)                    
                        return (T)field.GetValue(null);
                    
                
                else if (field.Name == description)                
                    return (T)field.GetValue(null);                
            }

            throw new ArgumentException("Not found.", nameof(description));
            // или возвращать значения по умолчанию
        }
    }

    public enum PageType
    {
        [Description("Поиск сервера")]
        FaindServer,

        [Description("Главное меню")]
        Main,

        [Description("Интро")]
        Intro,

        [Description("Частицы")]
        Partical,

        [Description("Фотоны")]
        Photons,

        [Description("Портал")]
        RightPortal,

        [Description("Портал")]
        LeftPortal
    }

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

    public interface IPage
    {
        //PageType Type { get; }
        Menu Page { get; }
    }

    public partial class MainPage : ContentPage
    {
        public static Dictionary<PageType, Menu> Menus = new Dictionary<PageType, Menu>
        {
            {
                PageType.Intro,
                null
            },
            {
                PageType.Main, 
                new Menu(
                        new Button(PageType.Partical, "Частицы"),
                        new Button(PageType.Photons, "Фотоны")
                )         
            },
            {
                PageType.Partical, 
                new Menu(
                        new Button(PageType.RightPortal, "Частицы", isActive: true),
                        new Button(PageType.RightPortal, "Открыть \nпортал")
                )
            },
            {
                PageType.Photons,
                new Menu(
                        new Button(PageType.LeftPortal, "Открыть \nпортал"),
                        new Button(PageType.LeftPortal, "Фотоны", isActive: true)
                )
            },
            {
                PageType.RightPortal,
                new Menu(
                        new Button(PageType.Main, "Главное \nменю"),
                        new Button(PageType.Main, "Продлить \nпортал", isActive: true)
                )
            },
            {
                PageType.LeftPortal,
                new Menu(
                        new Button(PageType.Main, "Продлить \nпортал", isActive: true),
                        new Button(PageType.Main, "Главное \nменю")
                )
            },
        };

        PageType CurrentPage = PageType.Main;
        List<Menu> _pages;       

        public MainPage()
        {
            InitializeComponent();
            SetPage(CurrentPage);
        }

        private void RightButton_Relesed(string obj)
        {            
            if (Menus[CurrentPage] != null)
            {
                var nextPage = Menus[CurrentPage].rightButton.Link;
                SetPage(nextPage);
            }
        }

        private void LeftButton_Relesed(string obj)
        {
            if (Menus[CurrentPage] != null)
            {
                var nextPage = Menus[CurrentPage].leftButton.Link;
                SetPage(nextPage);
            }            
        }

        void SetPage(PageType pageType)
        {
            LeftButton.Stop();
            RightButton.Stop();

            if (Menus[pageType] != null)
            {
                LeftButton.Text = Menus[pageType].leftButton.Text;
                RightButton.Text = Menus[pageType].rightButton.Text;

                if (Menus[pageType].leftButton.IsActive)                
                    LeftButton.Restart();

                if (Menus[pageType].rightButton.IsActive)
                    RightButton.Restart();
            }

            CurrentPage = pageType;
        }
    }
}
