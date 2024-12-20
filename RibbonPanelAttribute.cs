using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiangChi.MepDesign.Revit.Ribbons
{

    /// <summary>
    /// 自定义特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RibbonPanelAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">唯一标识码</param>
        /// <param name="name">名字</param>
        /// <param name="title">显示名称</param>
        public RibbonPanelAttribute(int index, string name, string title)
        {
            this.Id = index;
            this.Name = name;
            this.Title = title;
            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = "缺省";
            }
        }

        /// <summary>
        /// 唯一序号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
    }


}
