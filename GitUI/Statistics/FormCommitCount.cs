﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Statistics;

namespace GitUI.Statistics
{
    public partial class FormCommitCount : GitExtensionsForm
    {
        public FormCommitCount()
        {
            InitializeComponent();
            this.Loading.Image = global::GitUI.Properties.Resources.loadingpanel;
            Translate();
        }

        private void FormCommitCountFormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("commit-count");
        }

        private void FormCommitCountLoad(object sender, EventArgs e)
        {
            RestorePosition("commit-count");
            FetchData();

        }
        private void cbIncludeSubmodules_CheckedChanged(object sender, EventArgs e)
        {
            FetchData();
        }

        private void FetchData()
        {
            Loading.Visible = true;

            CommitCount.Text = "";
            var dict = new Dictionary<string, HashSet<string>>();
            var items = CommitCounter.GroupAllCommitsByContributor().Item1;
            if (cbIncludeSubmodules.Checked)
            {
                IList<string> submodules = Settings.Module.GetSubmodulesNames();
                GitModule submodule = new GitModule();
                foreach (var submoduleName in submodules)
                {
                    submodule.WorkingDir = Settings.Module.WorkingDir + submoduleName + Settings.PathSeparator.ToString();
                    if (submodule.ValidWorkingDir())
                    {
                        var submoduleItems = CommitCounter.GroupAllCommitsByContributor(submodule).Item1;
                        foreach (var keyValuePair in submoduleItems)
                        {
                            if (!dict.ContainsKey(keyValuePair.Key))
                                dict.Add(keyValuePair.Key, new HashSet<string>());
                            dict[keyValuePair.Key].Add(submodule.SubmoduleName);
                            if (items.ContainsKey(keyValuePair.Key))
                                items[keyValuePair.Key] += keyValuePair.Value;
                            else
                                items.Add(keyValuePair.Key, keyValuePair.Value);
                        }
                    }
                }
            }

            var sortedItems = from pair in items
                        orderby pair.Value descending 
                        select pair;

            foreach (var keyValuePair in sortedItems)
            {
                string submodulesList = "";
                if (dict.ContainsKey(keyValuePair.Key))
                {
                    var sub = dict[keyValuePair.Key];
                    if (sub.Count == 1)
                    {
                        foreach (var item in dict[keyValuePair.Key])
                            submodulesList = " [" + item + "]";
                    }
                    else
                        submodulesList = " [" + sub.Count.ToString() + " submodules]";
                }
                CommitCount.Text += string.Format("{0,6} - {1}{2}\r\n", keyValuePair.Value, keyValuePair.Key, submodulesList);
            }

            Loading.Visible = false;
        }
    }
}