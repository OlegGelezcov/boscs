public interface IListViewTemplateSelector
{
    TypeTemplateMapping SelectTemplateCore(TypeTemplateMapping[] availableTemplates, object dataItem);
}