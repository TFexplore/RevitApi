using Autodesk.Revit.UI;
using LiangChi.MepDesign.Revit.Ribbons;
using LiangChi.MepDesignCore.Common;
using LiangChi.MepDesignCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LiangChi.MepDesign.Revit
{
    /// <summary>
    /// Ribbon菜单帮助类
    /// </summary>
    public static class RibbonHelper
    {
        /// <summary>
        /// 命令可用判定
        /// </summary>
        public static IExternalCommandAvailability CommandAvalibilityClass;

        public static IExternalCommandAvailability AlwaysAvalibilityClass;

        /// <summary>
        /// 加载程序集中的命令到指定名称选项卡下
        /// </summary>
        /// <param name="uiapp">revit ui </param>
        /// <param name="tabName">选项卡名称</param>
        /// <param name="assembly">程序集</param>
        /// <returns>新加载的面板</returns>
        public static List<RibbonPanel> CreateTabByAssembly(this UIApplication uiapp, string tabName, Assembly assembly)
        {
            var ribbonPanels = new List<RibbonPanel>();

            var panelKeyMap = LoadPanelData(assembly);

            var orderPanelLst = panelKeyMap.Keys.OrderBy(d => d.Id);
            // 面板按照序号排序后，加入到面板
            foreach (var panel in orderPanelLst)
            {
                var ribbonPanel = uiapp.GetRibbonPanelFrom(tabName, panel.Name);
                if (ribbonPanel == null && !string.IsNullOrEmpty(panel.Name))
                {
                    ribbonPanel = uiapp.CreateRibbonPanel(tabName, panel.Name);
                }
            }

            foreach (var kv in panelKeyMap)
            {
                var panel = kv.Key;
                var ribbonPanel = uiapp.GetRibbonPanelFrom(tabName, panel.Name);
                if (ribbonPanel != null)
                {
                    ribbonPanel.AddRibbonItem(kv.Value);
                }
            }

            return ribbonPanels;
        }

        /// <summary>
        /// 检索指定选项卡下的指定面板名的ribbon面板
        /// </summary>
        /// <param name="uiapp">revit ui</param>
        /// <param name="tabname">选项卡</param>
        /// <param name="panel">面板名</param>
        /// <returns>面板或null</returns>
        public static RibbonPanel GetRibbonPanelFrom(this UIApplication uiapp, string tabname, string panel)
        {
            try
            {
                var list = uiapp.GetRibbonPanels(tabname);
                if (null != list)
                {
                    return list.FirstOrDefault(d => d.Name == panel);
                }
            }
            catch (Exception)
            {
                uiapp.CreateRibbonTab(tabname);
            }

            return null;
        }

        /// <summary>
        /// 向指定面板中添加控件
        /// </summary>
        /// <param name="panel">面板</param>
        /// <param name="typelist">提供控件的类</param>
        public static void AddRibbonItem(this RibbonPanel panel, List<Type> typelist)
        {

            if (panel != null && typelist != null && typelist.Count > 0)
            {
                var list = typelist.Select(d => GetButtonTupe(d)).Where(d => d != null).ToList();
                if (list.Count > 0)
                {
                    var orderLst = list.OrderBy(d => { if (d.Item1 == null) return d.Item2.ColumnIndex; else return d.Item1.ColumnIndex; }).ThenBy(d => d.Item1 == null);

                    Dictionary<int, PulldownButton> pullMap = new Dictionary<int, PulldownButton>();


                    List<(int Row, PushButtonData Button)> cacheBtns = new List<(int Row, PushButtonData Button)>();
                    List<PushButtonAttribute> subBtnLst = new List<PushButtonAttribute>();
                    int lastIndex = -1;
                    foreach (var item in orderLst)
                    {
                        if (item.Item1 != null && item.Item1 is PulldownButtonAttribute attPull)
                        {
                            lastIndex = attPull.ColumnIndex;
                            if (!pullMap.ContainsKey(attPull.ColumnIndex))
                            {// 下拉按钮
                                var pullbutton = panel.AddPulldownButton(attPull);
                                if (null != pullbutton)
                                {
                                    pullMap.Add(attPull.ColumnIndex, pullbutton);
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            if (item.Item2 != null && item.Item2 is PushButtonAttribute attPush)
                            {
                                attPush.ColumnIndex = lastIndex;
                                subBtnLst.Add(attPush);
                            }
                        }
                        else if (item.Item2 != null && item.Item2 is PushButtonAttribute attPush)
                        {
                            if (attPush.RowIndex > 0)
                            { // 层叠按钮
                                var data = attPush.GetPushButtonData(CommandAvalibilityClass);
                                if (data != null)
                                {
                                    if (lastIndex != attPush.ColumnIndex && cacheBtns.Count > 0)
                                    {
                                        if (lastIndex != attPush.ColumnIndex)
                                        {
                                            panel.AddManyPushButton(cacheBtns.OrderBy(d => d.Row).Select(d => d.Button).ToList());
                                            cacheBtns.Clear();
                                        }
                                    }
                                    cacheBtns.Add((attPush.RowIndex, data));
                                    lastIndex = attPush.ColumnIndex;
                                    continue;
                                }
                            }
                            else
                            {
                                if (cacheBtns.Count > 0)
                                {
                                    panel.AddManyPushButton(cacheBtns.OrderBy(d => d.Row).Select(d => d.Button).ToList());
                                    cacheBtns.Clear();
                                }
                                panel.AddPushButton(attPush);
                            }

                        }

                    }

                    if (cacheBtns.Count > 0) // 层叠按钮
                        panel.AddManyPushButton(cacheBtns.OrderBy(d => d.Row).Select(d => d.Button).ToList());

                    if (subBtnLst.Count > 0)
                    {
                        foreach (var kv in pullMap)
                        {
                            var lst = subBtnLst.Where(d => d.ColumnIndex == kv.Key);
                            if (lst.Count() > 0)
                            {
                                lst.OrderBy(d => d.RowIndex).ForEach(d =>
                                {
                                    var data = (d.GetPushButtonData(CommandAvalibilityClass));
                                    if (null != data)
                                    {
                                        if (d.Separator)
                                            kv.Value.AddSeparator();
                                        kv.Value.AddPushButton(data);
                                    }
                                });
                            }
                        }
                    }

                }
            }
        }

        /// <summary>
        /// 向面板下添加下拉按钮
        /// </summary>
        /// <param name="panel">面板</param>
        /// <param name="pull">下拉描述</param>
        /// <returns>下拉按钮或null</returns>
        public static PulldownButton AddPulldownButton(this RibbonPanel panel, PulldownButtonAttribute pull)
        {
            if (null != pull && panel != null)
            {
                if (string.IsNullOrEmpty(pull.Name))
                {
                    pull.Name = pull.Text;
                }

                SplitButtonData data = new SplitButtonData(pull.Name, pull.Text);
                PulldownButton pulldownButton = panel.AddItem(data) as PulldownButton;
                if (pull != null)
                {
                    if (pull.Separator)
                    {
                        panel.AddSeparator();
                    }
                    if (pull.SlideOut)
                    {
                        panel.AddSlideOut();
                    }
                }
                return pulldownButton;
            }
            return null;
        }


        /// <summary>
        /// 向面板下添加按钮
        /// </summary>
        /// <param name="panel">面板</param>
        /// <param name="push">按钮描述</param>
        /// <returns>按钮或null</returns>
        public static PushButton AddPushButton(this RibbonPanel panel, PushButtonAttribute push)
        {
            if (null != push && panel != null)
            {

                PushButtonData data = push.GetPushButtonData(CommandAvalibilityClass);
                data.Image = ReadImage(push.Image);
                data.LargeImage = ReadImage(push.LargeImage);
                data.ToolTip = push.ToolTip;
                data.ToolTipImage = ReadImage(push.ToolTipImage);
                PushButton btn = panel.AddItem(data) as PushButton;
                if (push != null)
                {
                    if (push.Separator)
                    {
                        panel.AddSeparator();
                    }
                    if (push.SlideOut)
                    {
                        panel.AddSlideOut();
                    }
                }
                return btn;
            }
            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="list"></param>
        private static void AddManyPushButton(this RibbonPanel panel, List<PushButtonData> list)
        {
            while (list.Count() > 2)
            {
                var obj3 = list.Take(3).ToList();
                panel.AddStackedItems(obj3[0], obj3[1], obj3[2]);
                list.RemoveRange(0, 3);
            }

            if (list.Count == 2)
            {
                panel.AddStackedItems(list[0], list[1]);
            }
            else
            {
                var obj1 = list.FirstOrDefault();
                if (obj1 != null)
                    panel.AddItem(obj1);
            }

        }


        /// <summary>
        /// 类型转按钮数据
        /// </summary>
        /// <param name="push">类型</param>
        /// <param name="obj">可用性</param>
        /// <returns>按钮数据</returns>
        public static PushButtonData GetPushButtonData(this PushButtonAttribute push, IExternalCommandAvailability obj)
        {
            PushButtonData data = null;
            if (string.IsNullOrEmpty(push.Name))
            {
                push.Name = push.Text;
            }
            string assmbylename = push.CommandClass.Assembly.Location;
            string classname = push.CommandClass?.FullName ?? "无效命令";

            data = new PushButtonData(push.Name, push.Text, assmbylename, classname);

            data.Image = ReadImage(push.Image);
            data.LargeImage = ReadImage(push.LargeImage);
            data.ToolTip = push.ToolTip;
            data.ToolTipImage = ReadImage(push.ToolTipImage);
            if (null != obj)
                data.AvailabilityClassName = obj.GetType().FullName;
            if (push.IsAlwaysOk)
            {
                data.AvailabilityClassName = RibbonHelper.AlwaysAvalibilityClass?.GetType()?.FullName;
            }

            return data;

        }

        /// <summary>
        /// 读取图片委托， 输入参数为图片路径
        /// </summary>
        public static Func<string, ImageSource> ReadImageFunc;

        private static ImageSource ReadImage(string filePath)
        {
            ImageSource img = null;
            img = ReadImageFunc?.Invoke(filePath);
            return img;
        }


        /// <summary>
        /// 加载程序集中具有RibbonPanel特性的类，类必须时IExternalCommand实现类
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static Dictionary<RibbonPanelAttribute, List<Type>> LoadPanelData(Assembly assembly)
        {
            Dictionary<RibbonPanelAttribute, List<Type>> result = new Dictionary<RibbonPanelAttribute, List<Type>>();

            var list = assembly.GetTypes()
                .Where(t => t.GetCustomAttribute(typeof(RibbonPanelAttribute)) != null && typeof(IExternalCommand).IsAssignableFrom(t))
                .ToList();
            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    var att = item.GetCustomAttribute<RibbonPanelAttribute>();
                    var matched = result.Keys.FirstOrDefault(d => d.Id == att.Id);
                    if (matched != null)
                    {
                        result[matched].Add(item);
                    }
                    else
                    {
                        result.Add(att, new List<Type>() { item });
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 从类中提取按钮描述二元组
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>二元组(Pulldown,PushButton)或null</returns>
        private static Tuple<PulldownButtonAttribute, PushButtonAttribute> GetButtonTupe(Type type)
        {
            var left = type.GetCustomAttribute<PulldownButtonAttribute>();
            var right = type.GetCustomAttribute<PushButtonAttribute>();
            if (right != null)
            {
                right.CommandClass = type;
            }
            else return null;
            return new Tuple<PulldownButtonAttribute, PushButtonAttribute>(left, right);
        }
    }
}
