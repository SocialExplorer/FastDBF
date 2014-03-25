using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using SocialExplorer.IO.FastDBF;


namespace SocialExplorer.FastDBF.Tests.Unit
{
	[TestFixture]
    public class DbfColumnTests
    {
		# region Setup and Tear Down
		/// <summary>
		/// TestFixtureSetUp called once before any tests have been run in the same TestFixture
		/// </summary>
		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
		}

		/// <summary>
		/// TestFixtureTearDown called once after all tests have been run in the same TestFixture
		/// </summary>
		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
		}

		/// <summary>
		/// SetsUp is called once before each Test within the same TestFxiture
		/// If this throws an exception no Test in the TestFixture are run.
		/// </summary>
		[SetUp]
		public void SetUp()
		{
		}

		/// <summary>
		/// TearsDown is called once after each Test within the same TestFixture.
		/// Will not run if no tess are run due to [SetUp] throwing an exception
		/// </summary>
		[TearDown]
		public void TearDown()
		{
		}
		#endregion


		#region Tests


		#region Constructors
		[Test]
		[ExpectedException(typeof(Exception), ExpectedMessage = "Field names must be at least one char long and can not be null.")]
		public void DbfColumn_Construct_NameNull_ThrowsException()
		{
			//Assert.Throws<Exception>(() => new DbfColumn(null, default(DbfColumn.DbfColumnType)));
			var dbfColumn = new DbfColumn(null, default(DbfColumn.DbfColumnType));
		}
		#endregion
		#endregion
	}
}
