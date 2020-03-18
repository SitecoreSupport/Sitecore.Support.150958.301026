using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Shell.Framework;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Sites;
using Sitecore.Web.UI.Sheer;
using Sitecore.Workflows;
using System.Collections.Specialized;
using Sitecore.Text;

namespace Sitecore.Support.Shell.Framework.Commands
{
    public class PublishItem : Command
    {
        public override void Execute(CommandContext context)
        {
            if (context.Items.Length == 1)
            {
                Item item = context.Items[0];
                NameValueCollection nameValueCollection = new NameValueCollection();
                nameValueCollection["id"] = item.ID.ToString();
                nameValueCollection["language"] = item.Language.ToString();
                nameValueCollection["version"] = item.Version.ToString();
                nameValueCollection["workflow"] = "0";
                Context.ClientPage.Start(this, "Run", nameValueCollection);
            }
        }

        /// <summary>
        /// Queries the state of the command.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The state of the command.</returns>
        public override CommandState QueryState(CommandContext context)
        {
            if (context.Items.Length != 1 || !Settings.Publishing.Enabled)
            {
                return CommandState.Hidden;
            }
            return base.QueryState(context);
        }

        /// <summary>
        /// Runs the specified args.
        /// </summary>
        /// <param name="args">The arguments.</param>
        protected void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            string itemPath = args.Parameters["id"];
            string name = args.Parameters["language"];
            string value = args.Parameters["version"];
            if (SheerResponse.CheckModified(new CheckModifiedParameters
            {
                ResumePreviousPipeline = true
            }))
            {
                Item item = Context.ContentDatabase.Items[itemPath, Language.Parse(name), Sitecore.Data.Version.Parse(value)];
                if (item == null)
                {
                    SheerResponse.Alert("Item not found.");
                }
                else if (CheckWorkflow(args, item))
                {
                    Log.Audit(this, "Publish item: {0}", AuditFormatter.FormatItem(item));
                    Publish(item);
                }
            }
        }

        /// <summary>
        /// Checks the workflow.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="item">The item.</param>
        /// <returns>The workflow.</returns>
        private static bool CheckWorkflow(ClientPipelineArgs args, Item item)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(item, "item");
            if (args.Parameters["workflow"] == "1")
            {
                return true;
            }
            args.Parameters["workflow"] = "1";
            if (args.IsPostBack)
            {
                if (args.Result == "yes")
                {
                    args.IsPostBack = false;
                    return true;
                }
                args.AbortPipeline();
                return false;
            }
            SiteContext site = Factory.GetSite("publisher");
            if (site != null && !site.EnableWorkflow)
            {
                return true;
            }
            IWorkflowProvider workflowProvider = Context.ContentDatabase.WorkflowProvider;
            if (workflowProvider == null || workflowProvider.GetWorkflows().Length == 0)
            {
                return true;
            }
            IWorkflow workflow = workflowProvider.GetWorkflow(item);
            if (workflow == null)
            {
                return true;
            }
            WorkflowState state = workflow.GetState(item);
            if (state == null)
            {
                return true;
            }
            if (state.FinalState)
            {
                return true;
            }
            args.Parameters["workflow"] = "0";
            if (state.PreviewPublishingTargets.Any())
            {
                return true;
            }
            SheerResponse.Confirm(Translate.Text("The current item \"{0}\" is in the workflow state \"{1}\"\nand will not be published.\n\nAre you sure you want to publish?", item.DisplayName, state.DisplayName));
            args.WaitForPostBack();
            return false;
        }

        private static void Publish(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            //SheerResponse.CheckModified(response: false);
            UrlString urlString = new UrlString("/sitecore/shell/Applications/Publish.aspx");
            urlString.Append("id", item.ID.ToString());
            SheerResponse.Broadcast(SheerResponse.ShowModalDialog(urlString.ToString()), "Shell");
        }
    }
}