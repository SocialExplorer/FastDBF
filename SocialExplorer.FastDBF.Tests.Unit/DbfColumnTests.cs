#region Usings
using System;

using NUnit.Framework;

using SocialExplorer.IO.FastDBF;


#endregion


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

		/// <summary>
		/// TestFixtureTearDown called once after all tests have been run in the same TestFixture
		/// </summary>
		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
		}
		#endregion


		#region Tests


		#region Constructors
		[Test]
		[ExpectedException(typeof(Exception), ExpectedMessage = "Field names must be at least one char long and can not be null.")]
		public void DbfColumn_Construct_NameNull_ThrowsException()
		{
			var dbfColumn = new DbfColumn(null, default(DbfColumn.DbfColumnType));
		}

		[Test]
		[ExpectedException(typeof(Exception), ExpectedMessage = "Field names must be at least one char long and can not be null.")]
		public void DbfColumn_Construct_NameEmpty_ThrowsException()
		{
			var dbfColumn = new DbfColumn(string.Empty, default(DbfColumn.DbfColumnType));
		}
		[Test]
		[ExpectedException(typeof(Exception), ExpectedMessage = "Field names can not be longer than 11 chars.")]
		public void DbfColumn_Construct_NameLongerThan11Chars_ThrowsException()
		{
			var dbfColumn = new DbfColumn("TheseAreMoreThan11Chars", default(DbfColumn.DbfColumnType));
		}

		[Test]
		[ExpectedException(typeof(Exception), ExpectedMessage = "Invalid field length specified. Field length can not be zero or less than zero.")]
		public void DbfColumn_Construct_DbfColumnTypeNumber_NoLenghtNorDecimalPrecisionSpecified_ThrowsException()
		{
			var dbfColumn = new DbfColumn("COLUMN", DbfColumn.DbfColumnType.Number);
		}

		[Test]
		[ExpectedException(typeof(Exception), ExpectedMessage = "Invalid field length specified. Field length can not be zero or less than zero.")]
		public void DbfColumn_Construct_DbfColumnTypeNumber_NoLenghtNorDecimalPrecisionSpecified2_ThrowsException()
		{
			var dbfColumn = new DbfColumn("COLUMN", DbfColumn.DbfColumnType.Number, 0, 0);
		}

		[Test]
		[ExpectedException(typeof(Exception), ExpectedMessage = "Decimal precision can not be larger than the length of the field.")]
		public void DbfColumn_Construct_DbfColumnTypeNumber_LenghtBiggerThanDecimalPrecision_ThrowsException()
		{
			var dbfColumn = new DbfColumn("COLUMN", DbfColumn.DbfColumnType.Number, 1, 2);
		}

		#endregion
		#endregion
	}
}
