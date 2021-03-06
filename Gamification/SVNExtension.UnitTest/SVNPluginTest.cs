﻿using System;
using System.Diagnostics;
using System.Linq;
using DatabaseAccess;
using Extension;
using SVNExtension.Model;
using LanguageExtension;
using MongoDB.Bson.Serialization;
using NUnit.Framework;
namespace SVNExtension.UnitTest
{
    [TestFixture]
    public class SVNPluginTest
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(DefaultUser)))
            {
                BsonClassMap.LookupClassMap(typeof(DefaultUser));
            }            
            using (var process = new Process())
            {
                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.Arguments = ".\\BuildTests\\BuildSvnRepos.ps1";
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit(20000);
            }

        }

        [TearDown]
        public void TearDown()
        {
            CleanDatabase();
        }

        private void CleanDatabase()
        {
            var dbManager = new DatabaseManager();
            var testDatabase = dbManager.GetDatabase();
            testDatabase.DropCollection(typeof(IUser).Name);
            testDatabase.DropCollection(typeof(SVNRepository).Name);
        }

        [Test]
        public void TestSimplyAnalyze()
        {            
            var dbUsers = new DatabaseUsers();            
            var plugin = new SVNPlugin();
            var plugin2 = new LanguageExtension.LanguagePlugin();
            new LanguageExtension.LanguagePlugin().LoadDBMaps();            
            plugin.LoadDBMaps();
            plugin.Analyze();
            var manager = new DatabaseManager();
            var db = manager.GetDatabase();
            var collection = db.GetCollection<IUser>(typeof(IUser).Name);
            foreach (var user in collection.FindAll())
            {
                Assert.AreEqual(Environment.UserName, user.Name);
                Assert.AreEqual(20, ((SVNModel)user.ExtensionPoint["SVNExtension"]).Add);
                Assert.AreEqual(0, ((SVNModel)user.ExtensionPoint["SVNExtension"]).Modified);
                Assert.AreEqual(0, ((SVNModel)user.ExtensionPoint["SVNExtension"]).Deleted);
            }

            var repos = db.GetCollection<SVNRepository>(typeof(SVNRepository).Name);
            foreach (var repo in repos.FindAll())
            {
                Assert.AreEqual(2, repo.CurrentVersion);
            }
        }

        [Test]
        public void LevelUserUpTest()
        {
            string fileLevel = ".\\Experience\\UserLevel.prop";
            var model = new SVNModel();
            model.AddAdd(10);
            model.AddModified(5);                        
            var svnExp = new SVNExperience("TestSVNModel", fileLevel, "SVN");
            var exp = new Experience("TestUser", fileLevel, "SVN");
            svnExp.AddModel(model);
            exp.AddPluginExperience(svnExp);            
            Assert.AreEqual(2, exp.Level);
            Assert.AreEqual(15, exp.ExperiencePoints);
        }

        [Test]
        public void LevelSvnUpTest()
        {
            string fileLevel = ".\\Experience\\UserLevel.prop";
            var model = new SVNModel();
            model.AddAdd(1000);
            model.AddModified(10000);
            model.AddDeleted(600);
            var svnExp = new SVNExperience("TestUser", fileLevel, "SVN");
            svnExp.AddModel(model);
            Assert.AreEqual(17, svnExp.Level);
            Assert.AreEqual(11600, svnExp.ExperiencePoints);
            Assert.AreEqual("TestUser", svnExp.Name);
        }

        [Test]
        public void DatabaseRegistryLevelUpTest()
        {
            var plugin = new SVNPlugin();
            plugin.Analyze();
            plugin.Compute();
            var db = new DatabaseManager();            
            var database = db.GetDatabase();            
            var collection = database.GetCollection<IUser>(typeof(IUser).Name);
            foreach (var user in collection.FindAll())
            {
                var exp = user.ExperiencePoints[typeof(SVNExperience).Name];                
                Assert.AreEqual(Environment.UserName, exp.Name);
                Assert.AreEqual(2, exp.Level);
                Assert.AreEqual(20, exp.ExperiencePoints);
                
            }
        }

        /// <summary>
        /// this tests must be here because language plugin depends on other plugins
        /// to insert he dates
        /// </summary>
        [Test]
        public void languagePluginlvlUpVerifyDatabaseInsertTest()
        {
            var plugin = new SVNPlugin();
            plugin.Analyze();            

            var langPlugin = new LanguagePlugin();
            langPlugin.Compute();

            var collection = new DatabaseManager()
                .GetDatabase()
                .GetCollection<IUser>(typeof(IUser).Name);

            foreach (var user in collection.FindAll())
            {
                var exp = user.ExperiencePoints[typeof(LanguageExperience).Name];
                Assert.AreEqual(Environment.UserName, exp.Name);
                Assert.AreEqual(2, exp.Level);
                Assert.AreEqual(20, exp.ExperiencePoints);
            }
            

        }
    }
}
