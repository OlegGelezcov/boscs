using System.Linq;
public class DefaultListViewTemplateSelector : IListViewTemplateSelector
{
    public virtual TypeTemplateMapping SelectTemplateCore(TypeTemplateMapping[] availableTemplates, object dataItem)
    {
        return availableTemplates.FirstOrDefault(q => q.TypeName == dataItem.GetType().Name);
    }
}
