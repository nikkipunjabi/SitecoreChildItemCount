# About Sitecore Child Item Count

Sitecore Child Item Count Module is for getting the child item of the selected item in Content Editor. It also provide an option to specify the template id of child item, if you want to fetch the items of any specific template. It works based on the index. It also check the current Language. If your current selected language is Japanese and if the child item doesn't exist with Japanese language version than it won't include in child count.

# Sitecore Package

[Download Package](http://nikkipunjabi.com/Sitecore/ChildItemCount/Sitecore%20Child%20Item%20Count-0.0.0.1.zip)

This package contains below files and item.
 - Sitecore.SharedSource.ChildItemCount.config
 - Sitecore.SharedSource.ChildItemCount.Commands.ChildItemCount.cs
 - Child Count - Item which is stored in Core DB

In order to use this module you just need to install the module and you are done. It will install above files and item in your sitecore solution.

### Output

![Child Count](http://nikkipunjabi.com/Sitecore/ChildItemCount/1-Right_Click-Child_Count.png "Child Count in Context Menu")
![Output](http://nikkipunjabi.com/Sitecore/ChildItemCount/2-Output.png "Output")
![Output](http://nikkipunjabi.com/Sitecore/ChildItemCount/3-Output.png "Output")

### Error

If the child items are more than 4096 then you might get an below error.
![Error](http://nikkipunjabi.com/Sitecore/ChildItemCount/4-Error.png "Output")

In order to handle this you will need to increase value of LuceneQueryClauseCount in Sitecore.ContentSearch.Lucene.DefaultIndexConfiguration.config

![Error_Solution](http://nikkipunjabi.com/Sitecore/ChildItemCount/5-Error_Solution.png "Error_Solution")

### Extension Method

I've also created an below extension method, so you can use and modify as per requirement.

```

public static class ItemExtension
    {
        public static int GetallItemsCount(this Item item, string itemPath, string templateID = null)
        {
            var indexable = (SitecoreIndexableItem)item;
            using (var context = ContentSearchManager.GetIndex(indexable).CreateSearchContext())
            {
                List<SearchResultItem> results = null;
                if (string.IsNullOrWhiteSpace(templateID))
                {
                    results = context.GetQueryable<SearchResultItem>().Where(x => x.Path.StartsWith(itemPath + "/") && x.Language == item.Language.Name).ToList();
                }
                else
                {
                    Guid guidOutput = new Guid();
                    bool isTemplateIDGUID = Guid.TryParse(templateID, out guidOutput);
                    if (isTemplateIDGUID)
                    {
                        //Applied Language Check because it was fetching same item for all the different languages if any.
                        //In this case -- If the child item doesn't exist with the current selected Language then it won't be displayed.
                        results = context.GetQueryable<SearchResultItem>().Where(x => x.Path.StartsWith(itemPath + "/")).ToList();
                        results = results.Where(x => x.TemplateId == new ID(templateID) && x.Language == item.Language.Name).ToList();
                    }
                }
                if (results != null && results.Count > 0)
                {
                    foreach (var result in results)
                    {
                        var res = result.GetItem();
                    }
                    return results.Count;
                }

                return 0;
            }
        }
    }
  ```

Happy Counting! :)
