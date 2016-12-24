using Sitecore.Buckets.Managers;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sitecore.SharedSource.ChildItemCount.Commands
{
    public class ChildItemCount : Command
    {
        //Display Name: Child Count
        //Icon: Applications/32x32/navigate_close.png
        //Message: item:getchilditemcount(id=$Target,title=Do you want to specify Template ID?)
        Item currentItem = null;
        public override void Execute(CommandContext context)
        {
            if (context.Items.Length == 1)
            {
                currentItem = context.Items[0];
                Context.ClientPage.Start(this, "Confirm", context.Parameters);
            }
        }

        public override CommandState QueryState(CommandContext context)
        {
            if (context.Items.Length == 1)
            {
                Item currentItem = context.Items[0];
                if (currentItem.Children.Count == 0)
                {
                    return CommandState.Hidden;
                }
            }
            return base.QueryState(context);
        }

        #region Child Count

        protected void Confirm(ClientPipelineArgs args)
        {
            try
            {
                if (args.IsPostBack)
                {
                    if (args.Result == "undefined")
                    {
                        //Do Nothing
                    }
                    else if (!string.IsNullOrWhiteSpace(args.Result))
                    {
                        string result = ItemCount(args.Result);
                        if (result.StartsWith("No result found."))
                        {

                            if (!string.IsNullOrWhiteSpace(args.Parameters["Error"]))
                            {
                                args.Parameters["Error"] = result;
                            }
                            else
                            {
                                args.Parameters.Add("Error", result);
                            }
                            ShowResult(args);
                        }
                        else
                        {
                            SheerResponse.Alert(result, false);
                        }
                    }
                }
                else
                {
                    ShowResult(args);
                }
            }
            catch (Exception ex)
            {
                SheerResponse.Alert("There was an error. " + ex.Message);
            }
        }

        public string ItemCount(string templateID = "")
        {
            var isBucketItemFolder = BucketManager.IsBucket(currentItem);
            string responseMessage = string.Empty;
            int count = 0;

            count = currentItem.GetallItemsCount(currentItem.Paths.FullPath, templateID);
            if (count > 0 && isBucketItemFolder)
            {
                responseMessage = string.Format("This is a Bucket Folder.\n Child Item Count: {0}", count.ToString());
                if (!(string.IsNullOrWhiteSpace(templateID)))
                {
                    responseMessage += "\n Template ID: " + templateID;
                }
            }
            else if (count > 0)
            {
                responseMessage = string.Format("Child Item Count: {0}", count.ToString());
                if (!(string.IsNullOrWhiteSpace(templateID)))
                {
                    responseMessage += "\n Template ID: " + templateID;
                }
            }
            else
            {
                responseMessage = string.Format("No result found. please try again!");
            }
            return responseMessage;
        }

        private void ShowResult(ClientPipelineArgs args)
        {
            string result = ItemCount();
            if (result.StartsWith("No result found."))
            {
                SheerResponse.Alert(result, false);
            }
            else if (!string.IsNullOrWhiteSpace(args.Parameters["Error"]))
            {
                if (!string.IsNullOrWhiteSpace(args.Parameters["title"]))
                {
                    result += "\n" + args.Parameters["title"];
                }
                else
                {
                    result += "\n Do you want to specify the template id?";
                }

                result += "\n" + args.Parameters["Error"];
                SheerResponse.Input(result, "");
                args.WaitForPostBack();

            }
            else
            {
                if (!string.IsNullOrWhiteSpace(args.Parameters["title"]))
                {
                    result += "\n" + args.Parameters["title"];
                }
                else
                {
                    result += "\n Do you want to specify the template id?";
                }

                SheerResponse.Input(result, "");
                args.WaitForPostBack();
            }
        }

        #endregion

    }

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
}
