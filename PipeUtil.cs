using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using xxx.MepDesign.Revit;
using xxx.MepDesign.WaterApplication.Model3.Factorys;
using xxx.MepDesign.WaterApplication.Model3.Service.Ylps;
using xxx.MepDesign.WaterApplication.PlanPlaces.Datas;

namespace xxx.MepDesign.WaterApplication.Model3.Utils
{
    public class PipeUtil
    {
        private static Document doc { get; set; }

        public static PipingSystemType pipingSystemType { get; set; }

        public static PipeType pipeType;

        private static List<PipingSystemType> PipeSysTypes { get; set; }
        private static List<PipeType> PipeTypes { get; set; }

        public static void Refresh(Document document,string pipeSys= "压力排水")
        {
            doc = document;

            FilteredElementCollector filter = new FilteredElementCollector(doc);
            // 获取管道系统类型
            PipeSysTypes = new FilteredElementCollector(doc).OfClass(typeof(PipingSystemType)).Cast<PipingSystemType>().ToList();
            pipingSystemType = PipeSysTypes.FirstOrDefault(i => i.Name.Contains(pipeSys));
            filter = new FilteredElementCollector(doc);
            PipeTypes = filter.OfClass(typeof(PipeType)).WhereElementIsElementType().OfCategory(BuiltInCategory.OST_PipeCurves).Cast<PipeType>().ToList();
        }

        public static void CreatPipe(List<PipeModel> models)
        {
            foreach (var model in models)
            {
                if (model.Type is PipeType pipeType)
                {
                    try
                    {
                        Pipe pipe = Pipe.Create(doc, pipingSystemType.Id, pipeType.Id, model.OwnLevel.Id, model.StartPoint, model.EndPoint);
                        设置管道必要参数(model, pipe);
                        model.RevitInstance = pipe;
                    }
                    catch
                    {

                    }

                }
                else
                {
                    var sy = model.Type as FamilySymbol;
                    if (sy.Name.Contains("套管"))
                    {

                    }
                    sy?.Activate();
                    var centerP = model.StartPoint;
                    if (model.StartPoint != null && model.EndPoint != null) centerP = (model.EndPoint + model.StartPoint) / 2;
                    var ins = doc.Create.NewFamilyInstance(centerP, sy,model.OwnLevel, StructuralType.NonStructural);
                    Line axis = Line.CreateUnbound(model.StartPoint, XYZ.BasisZ); // 旋转轴,沿着Z轴
                    double angle = model.Rotation;
                    if (model.NeedXZ)
                    {
                        var rs = CalculateRotation(ins, model.StartPoint, model.EndPoint);
                        if (!rs.axis.IsZeroLength()) {
                            axis = Line.CreateUnbound(centerP, rs.axis);
                            angle = rs.angle;
                        }                     
                    }
                    // 将角度从度转换为弧度,这里是45度
                    ElementTransformUtils.RotateElement(doc, ins.Id, axis, angle);
                    var translationVector = centerP- (ins.Location as LocationPoint).Point;
                    ElementTransformUtils.MoveElement(doc, ins.Id, translationVector);

                    TrySetDiameter(ins, model.Diameter / 304.8);

                    model.RevitInstance = ins;
                }

            }
            return;
        }

        public static void Dispose()
        {
            doc = null;
            PipeSysTypes = null;
            PipeTypes = null;
        }
        /// <summary>
        /// 生成弯头,三通时,e1和e2应在同一方向
        /// </summary>
        /// <param name="element1"></param>
        /// <param name="element2"></param>
        /// <param name="element3"></param>
        public static FamilyInstance Connect(Element element1, Element element2, Element element3 = null)
        {
            var rs1 = GetClosestConnectors(element1, element2);
            var rs2 = GetClosestConnectors(element1, element3);

            if (rs2 != null && rs1 != null)
            {
                try
                {
                    return doc.Create.NewTeeFitting(rs1.Item1, rs1.Item2, rs2.Item2);
                }
                catch(Exception ex)
                {

                }

            }
            else if (rs1 != null)
            {
                try
                {
                    return doc.Create.NewElbowFitting(rs1.Item1, rs1.Item2);
                }
                catch(Exception ex)
                {

                }
                try
                {
                    rs1.Item1.ConnectTo(rs1.Item2);
                }
                catch(Exception ex)
                {

                }
            }
            return null;
        }

        public static Connector GetConnectorByOrigin(Element element, XYZ origin)
        {
            ConnectorManager connectorManager = null;
            if (element is MEPCurve mEPCurve) connectorManager = mEPCurve.ConnectorManager;

            if (element is FamilyInstance instance) connectorManager = instance.MEPModel.ConnectorManager;

            if (connectorManager == null) return null;

            foreach (Connector connector in connectorManager.Connectors)
            {
                if (connector.Origin.IsAlmostEqualTo(origin))
                {
                    return connector;
                }
            }
            return null;
        }
        public static Connector GetConnectorById(Element element, int id)
        {
            ConnectorManager connectorManager = null;
            if (element is MEPCurve mEPCurve) connectorManager = mEPCurve.ConnectorManager;

            if (element is FamilyInstance instance) connectorManager = instance.MEPModel.ConnectorManager;

            if (connectorManager == null) return null;

            foreach (Connector connector in connectorManager.Connectors)
            {
                if (connector.Id==id)
                {
                    return connector;
                }
            }
            return null;
        }
        /// <summary>
        /// 获取两个最近的连接器
        /// </summary>
        /// <param name="element1"></param>
        /// <param name="element2"></param>
        /// <param name="offset">误差范围,0为不考虑误差</param>
        /// <returns></returns>
        public static Tuple<Connector, Connector> GetClosestConnectors(Element element1, Element element2, double offset = 0)
        {
            ConnectorManager connectorManager1 = null;
            ConnectorManager connectorManager2 = null;

            if (element1 is MEPCurve mEPCurve1) connectorManager1 = mEPCurve1.ConnectorManager;
            if (element1 is FamilyInstance instance1) connectorManager1 = instance1.MEPModel.ConnectorManager;

            if (element2 is MEPCurve mEPCurve2) connectorManager2 = mEPCurve2.ConnectorManager;
            if (element2 is FamilyInstance instance2) connectorManager2 = instance2.MEPModel.ConnectorManager;

            if (connectorManager1 == null || connectorManager2 == null) return null;

            Connector closestConnector1 = null;
            Connector closestConnector2 = null;
            double minDistance = double.MaxValue;

            foreach (Connector connector1 in connectorManager1.Connectors)
            {
                foreach (Connector connector2 in connectorManager2.Connectors)
                {
                    double distance = connector1.Origin.DistanceTo(connector2.Origin);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestConnector1 = connector1;
                        closestConnector2 = connector2;
                    }
                }
            }
            if (offset != 0 && minDistance > offset) return null;
            if (closestConnector1 == null || closestConnector2 == null) return null;

            return new Tuple<Connector, Connector>(closestConnector1, closestConnector2);
        }
        public static void 设置管道必要参数(PipeModel input, Pipe ans)
        {
            ans.SetSystemType(pipingSystemType.Id);
            ans.LookupParameter(GlobalVariables.FORWARD_DESIGN_SYSTEM_STRING)?.TrySet(pipingSystemType.Name);
            ans.LookupParameter(GlobalVariables.PARAM_AUTO_MODEL_BOOLEAN)?.TrySet(true);
            //ans.LookupParameter(GlobalVariables.ANOTATION_STRING)?.TrySet(input.DesignId?.ToString());
            //ans.LookupParameter(GlobalVariables.CHILD_SYSMTE_STRING)?.TrySet(input.ChildSystem);
            ans.LookupParameter(GlobalVariables.INSTALL_PRO_SYSTEMNAME)?.TrySet(pipingSystemType.Name);
            if (input.Diameter > 0)
                TrySetDiameter(ans, input.Diameter / 304.8);
            else
            {
                TrySetDiameter(ans, 25 / 304.8);
            }
            //ans.SetEntity(ModelingEntityFactory.CreateBy(input));
        }
        /// <summary>
        /// 尝试设置直径到非管件实例
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="value">直径</param>
        /// <returns></returns>
        public static bool TrySetDiameter(Element instance, double value)
        {
            if (value == 0) value = 15 / 304.8;

            if (instance != null)
            {
                foreach (string name in GlobalVariables.半径参数集合)
                {
                    if (instance.LookupParameter(name).TrySet(value / 2.0))
                    {
                        return true;
                    }
                }
                foreach (string name in GlobalVariables.直径参数集合)
                {
                    if (instance.LookupParameter(name).TrySet(value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static (XYZ axis, double angle) CalculateRotation(FamilyInstance instance, XYZ connector1, XYZ connector2)
        {
            XYZ[] cns = new XYZ[2];
            var connectorManager = instance.MEPModel.ConnectorManager;
            int index = 0;
            foreach (Connector connector in connectorManager.Connectors)
            {
                cns[index] = connector.Origin;
                index++;
            }
            // 初始方向向量 (沿X轴)
            XYZ v0 = cns[0] - cns[1];

            // 目标方向向量
            XYZ v1 = connector2 - connector1;

            // 计算旋转轴（叉积）
            XYZ axis = v0.CrossProduct(v1);
            axis = axis.Normalize(); // 规范化旋转轴

            // 计算旋转角度（点积）
            double dotProduct = v0.DotProduct(v1);
            double v1Length = v1.GetLength();
            double angle = Math.Acos(dotProduct / v1Length);
            return (axis, angle);
        }
        public static bool IsSameDirection(XYZ vector1, XYZ vector2)
        {
            // 检查向量之间的角度是否为零或接近零
            double angle = vector1.AngleTo(vector2) % Math.PI;
            if (Math.Abs(angle) < 0.001 || Math.Abs(Math.PI - angle) < 0.001) // 允许一个很小的误差
            {
                return true;
            }
            return false;
        }
    }
}
