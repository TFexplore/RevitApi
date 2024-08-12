# Revit API - 三维物体检测与点位检测

本文档介绍了如何使用Revit API进行三维物体检测、点位检测以及链接文档的查询。

## 1. 三维物体检测

此部分代码用于通过Revit API查找链接文档中的3D视图，并使用`ReferenceIntersector`进行三维物体的检测。

```csharp
// 获取第一个链接的文档
var linkDoc = LinkQueryUtil.QueryLinkedDocuments(doc).FirstOrDefault();

// 查找文档中的3D视图，排除名称包含"副本"的视图
FilteredElementCollector collector = new FilteredElementCollector(linkDoc);
var view3D = collector.OfClass(typeof(View3D))
                      .Cast<View3D>()
                      .Where(v => !v.Name.Contains("副本"))
                      .FirstOrDefault(v => !v.IsTemplate);

// 过滤结构构件、墙、柱等元素
FilteredElementCollector cltora = new FilteredElementCollector(linkDoc, view3D.Id)
    .WherePasses(new ElementMulticategoryFilter(new[] {
        BuiltInCategory.OST_StructuralFraming,
        BuiltInCategory.OST_Walls,
        BuiltInCategory.OST_StructuralColumns,
        BuiltInCategory.OST_Columns
    }))
    .WhereElementIsNotElementType();

// 使用ReferenceIntersector进行物体检测
var intersector = new ReferenceIntersector(cltora.ToElementIds(), FindReferenceTarget.All, view3D)
{
    FindReferencesInRevitLinks = true
};

// 查找最近的参考对象
var referenceWithContext = intersector.FindNearest(item.EndPoint, d);
Reference reference = referenceWithContext.GetReference();
Element element = linkDoc.GetElement(reference);

// 获取检测点的全局坐标
XYZ point = reference.GlobalPoint;
```
# 2. 点位检测
此部分代码用于检查某个点是否位于指定线段上，并通过判断管道端点是否在指定范围内来确定相交情况。

## 2.1 判断点是否在线段上
```csharp
private bool IsPointOnLine(PipeModel source, XYZ point)
{
    double tolerance = 0.001;
    if (source.StartPoint.DistanceTo(point) < tolerance || source.EndPoint.DistanceTo(point) < tolerance)
    {
        return false;
    }
    Line line = Line.CreateBound(source.StartPoint, source.EndPoint);
    XYZ projectedPoint = line.Project(point).XYZPoint;
    return projectedPoint.DistanceTo(point) < tolerance;
}

private bool IsPointOnLine(Curve source, XYZ point)
{
    double tolerance = 0.001;
    var p = point.SetZ(source.CenterPoint().Z);
    XYZ projectedPoint = source.Project(p).XYZPoint;
    return projectedPoint.DistanceTo(p) < tolerance;
}
```
## 2.2 判断端点是否相交
```csharp
public static long 判断端点是否相交(PipeModel source, List<(XYZ min, XYZ max, long Id)> targets, bool withZ = false, bool JustEnd = false)
{
    foreach (var target in targets)
    {
        if (JustEnd)
        {
            if (IsPointWithinBounds(source.EndPoint, target.min, target.max, withZ))
            {
                return target.Id;
            }
        }
        else
        {
            if (IsPointWithinBounds(source.StartPoint, target.min, target.max, withZ) ||
                IsPointWithinBounds(source.EndPoint, target.min, target.max, withZ))
            {
                return target.Id;
            }
        }
    }
    return 0;
}

// 判断点是否在指定的最小和最大坐标范围内
private static bool IsPointWithinBounds(XYZ point, XYZ min, XYZ max, bool withZ = false)
{
    if (withZ)
    {
        return (point.X >= min.X && point.X <= max.X &&
                point.Y >= min.Y && point.Y <= max.Y && Math.Abs(point.Z - max.Z) < 0.1);
    }
    return (point.X >= min.X && point.X <= max.X &&
            point.Y >= min.Y && point.Y <= max.Y);
}
```
# 3. 查询链接文档
此部分代码用于查询当前文档中的所有链接文档。
```csharp
/// <summary>
/// 查询当前文档中的所有链接文档
/// </summary>
/// <param name="doc">当前文档</param>
/// <returns>返回所有链接文档的列表</returns>
public static List<Document> QueryLinkedDocuments(Document doc)
{
    List<Document> result = new List<Document>();
    // 获取所有的链接文件
    FilteredElementCollector collector = new FilteredElementCollector(doc);
    collector.OfClass(typeof(RevitLinkInstance));
    var linkInstances = collector.ToElements().Cast<RevitLinkInstance>().ToList();
    foreach (var item in linkInstances)
    {
        // 获取链接文件的文档
        Document linkDoc = item.GetLinkDocument();
        result.Add(linkDoc);
    }
    return result;
}
```
总结
这些代码片段展示了如何在Revit API中执行三维物体检测、点位检测以及链接文档查询。可以根据实际需求进一步优化和扩展这些功能。
