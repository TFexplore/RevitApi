using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using xxx.MepDesign.Revit;
using xxx.MepDesignCore.Repository.Entity;
using xxx.MepDesignCore.Repository.Entity.压力排水;

namespace xxx.Application.Model3.Service.Ylps
{
    public class PipeModel
    {
        public long Id { get; set; }

        public XYZ StartPoint { get; set; }

        public XYZ EndPoint { get; set; }

        public Element RevitInstance { get; internal set; }

        public bool IsLiGuan { get; set; }
        public Element Type { get; set; }

        public Level OwnLevel { get; set; }

        public double Diameter { get; set; }

        public long LegendId { get; set; }

        public double Rotation { get; set; }
        //旋转轴,默认Z轴
        public Line axis;

        public bool NeedXZ = false;
        public bool isValid = true;
        public PipeModel()
        {
        }
        public PipeModel(PipeModel model,XYZ start,XYZ end)
        {
            Id = model.Id;
            StartPoint = start;
            EndPoint = end;

            OwnLevel = model.OwnLevel;
            Diameter=model.Diameter;
            Type = model.Type;
        }
    }
}
