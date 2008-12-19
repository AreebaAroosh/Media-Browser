﻿using System;
using System.Collections.Generic;
using System.Text;
using MediaBrowser.Library.Providers;
using MediaBrowser.Library;
using MediaBrowser.Library.Sources;
using MediaBrowser.LibraryManagement;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.MediaCenter.UI;
using System.Reflection;
using System.IO;

namespace MediaBrowserTest
{
    // extension class for testing
    public static class MyExtension
    {

        internal static ItemSource GetChild(this ItemSource source, int index)
        {

            foreach (var item in source.ChildSources)
            {
                if (item.RawName.Contains(".svn"))
                {
                    continue;
                }
                if (index == 0)
                {
                    return item;
                }
                index--;
            }
            return null;
        } 
    }

    [TestClass]
    public class TestTVDBMetadata
    {
        [TestInitialize]
        public void Setup()
        {
            typeof(Application).GetMethod("RegisterUIThread",BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
        }

        [TestMethod]
        public void TestEpisodeNumberFromFile()
        {
            Assert.AreEqual("02", Helper.EpisodeNumberFromFile(@"c:\somefolder\South.Park.s01e02 BFOD.avi"));
            Assert.AreEqual("05", Helper.EpisodeNumberFromFile(@"c:\somefolder\South.Park.01x05 BFOD.avi"));
            Assert.AreEqual("02", Helper.EpisodeNumberFromFile(@"c:\seriesname\season1\South.Park.s01e02 BFOD.avi"));
            Assert.AreEqual("05", Helper.EpisodeNumberFromFile(@"c:\seriesname\season 1\South.Park.01x05 BFOD.avi"));
            Assert.AreEqual("2", Helper.EpisodeNumberFromFile(@"c:\someseries\Season 1\2 - 22 Balloon.avi"));
            Assert.AreEqual("2", Helper.EpisodeNumberFromFile(@"c:\someseries\Saison 1\2 - 22 Balloon.avi"));
            Assert.AreEqual("2", Helper.EpisodeNumberFromFile(@"c:\someseries\Temporada 1\2 - 22 Balloon.avi"));
            Assert.AreEqual("2", Helper.EpisodeNumberFromFile(@"c:\someseries\Sæson 1\2 - 22 Balloon.avi"));
            Assert.AreEqual("03", Helper.EpisodeNumberFromFile(@"c:\someseries\Season1\103 BFOD.avi"));   

            // Test network share
            Assert.AreEqual("11", Helper.EpisodeNumberFromFile(@"\\10.0.0.4\videos\TV\Mister TV\Season 12\Mister.Tv.S12E11.NONSE"));
        }

        [TestMethod]
        public void TestSeasonFromEpisodeName()
        {
            Assert.AreEqual("12", Helper.SeasonNumberFromEpisodeFile(@"c:\videos\TV\South Park\Season 12\South.Park.S12E11.OTHERSTUFF.avi"));
        }

        [TestMethod]
        public void TestSourceNavigation()
        {
            var path = Path.GetFullPath(@"..\..\..\TestMediaBrowser\SampleMedia\TV");
            var source = new FileSystemSource(@"..\..\..\TestMediaBrowser\SampleMedia\TV");

            var item = source.GetChild(0); 
            Assert.AreEqual(ItemType.Series, item.ItemType);

            item = item.GetChild(0); 
            Assert.AreEqual(ItemType.Season, item.ItemType);

            item = item.GetChild(0);
            Assert.AreEqual(ItemType.Episode, item.ItemType);
        }


        [TestMethod]
        public void TestStandardFile()
        {
            var provider = new TvDbProvider();
            var store = new MediaMetadataStore(new UniqueName("TestTVDB"));
            Item item = new Item();

            var path = Path.GetFullPath(@"..\..\..\TestMediaBrowser\SampleMedia\TV");

            item.Assign(new FileSystemSource(path));
            item.EnsureChildrenLoaded(false);
            item.EnsureMetadataLoaded();

 //           var i2 = item.Children[0].Children[0].Children[0];

            Assert.IsTrue(provider.NeedsRefresh(item, ItemType.Episode));

            provider.Fetch(item, ItemType.Episode, store, false);
        }

        [TestMethod]
        public void TestRemoveNameCommments()
        {
            Assert.AreEqual("Hello", Helper.RemoveCommentsFromName("Hello[Comment]"));
            Assert.AreEqual("Hello World.avi", Helper.RemoveCommentsFromName("[Comment]Hello[Comment] World[Comment].avi"));
        }
    }
}