using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Silancer.Data;
using Fclp;
using System.Linq;
using Terminal.Gui;
using System.Reflection;

namespace Silancer
{
    public sealed class Program
    {
        static void Main(string[] args)
        {
            if (System.Globalization.CultureInfo.CurrentCulture.LCID != 2052)
                return;
            Application.Init();
            var top = Application.Top;

            #region Lancers面板
            var lancersWindow = new Window("Lancers")
            {
                X = 0,
                Y = 0,
                Width = Dim.Percent(10),
                Height = Dim.Percent(100) - 3
            };
            top.Add(lancersWindow);
            var lancersList = new RadioGroup(0, 0, new NStack.ustring[] { "Survivol10" });
            lancersWindow.Add(lancersList);
            lancersWindow.Remove(lancersList);
            //lancersWindow.Add(new RadioGroup(0, 0, new NStack.ustring[] { "Survivol101"}));
            #endregion

            #region Enemies面板
            var enemiesWindow = new Window("Enemies")
            {
                X = Pos.Percent(10),
                Y = 0,
                Width = Dim.Percent(10),
                Height = Dim.Percent(100) - 3
            };
            top.Add(enemiesWindow);
            #endregion

            #region 监视面板
            var inspectorWindow = new Window("监视面板")
            {
                X = Pos.Percent(20),
                Y = 0,
                Width = Dim.Percent(80),
                Height = Dim.Percent(50)
            };
            top.Add(inspectorWindow);
            #endregion

            #region 
            #endregion

            #region 输入区域
            var inputWindow = new Window("发布命令")
            {
                X = 0,
                Y = Pos.Bottom(top) - 3,
                Width = Dim.Fill(),
                Height = 3
            };
            var inputField = new TextField("")
            {
                X = 1,
                Width = Dim.Fill()
            };
            inputWindow.Add(inputField);
            top.Add(inputWindow);
            #endregion

            Application.Run();
        }
    }
}