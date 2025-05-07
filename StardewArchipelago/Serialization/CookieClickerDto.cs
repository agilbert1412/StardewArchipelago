using System;

namespace StardewArchipelago.Serialization
{
    public class CookieClickerDto
    {
        public double Cookies { get; set; }
        public int CursorUpgrades { get; set; }
        public int Grandmas { get; set; }

        public CookieClickerDto()
        {
            Cookies = 0;
            CursorUpgrades = 0;
            Grandmas = 0;
        }

        public int GetCookies()
        {
            return (int)Math.Floor(Cookies);
        }

        public void SpendCookies(int amount)
        {
            SpendCookies((double)amount);
        }

        public void SpendCookies(double amount)
        {
            Cookies -= amount;
        }

        public double GetCookiesPerClick()
        {
            return 1.0 * Math.Pow(2.0, CursorUpgrades);
        }

        public double GetCookiesPerSecond()
        {
            return GetBaseCookiesPerSecond() + GetGrandmaCookiesPerSecond();
        }

        private double GetBaseCookiesPerSecond()
        {
            return 0;
        }

        private double GetGrandmaCookiesPerSecond()
        {
            return 1 * Grandmas;
        }

        public double GetCookiesPerFrame()
        {
            return GetCookiesPerSecond() / 60d;
        }

        public void DoFrame()
        {
            Cookies += GetCookiesPerFrame();
        }

        public void ClickCookie()
        {
            Cookies += GetCookiesPerClick();
        }

        public void UpgradeCursor()
        {
            var price = GetCursorUpgradePrice();
            if (Cookies >= price)
            {
                SpendCookies(price);
                CursorUpgrades++;
            }
        }

        public void UpgradeGrandma()
        {
            var price = GetGrandmaUpgradePrice();
            if (Cookies >= price)
            {
                SpendCookies(price);
                Grandmas++;
            }
        }

        public int GetCursorUpgradePrice()
        {
            return (int)Math.Floor(10 * Math.Pow(5, CursorUpgrades));
        }

        public int GetGrandmaUpgradePrice()
        {
            return (int)Math.Floor(100 * Math.Pow(1.1, Grandmas));
        }
    }
}
