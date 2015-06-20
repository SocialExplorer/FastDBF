///
/// Author: Ahmed Lacevic
/// Date: 12/1/2007
/// Desc: 
/// 
/// Revision History:
/// -----------------------------------
///   Author:
///   Date:
///   Desc:


using System;
using System.Collections.Generic;
using System.Text;
using System.IO;



namespace SocialExplorer.IO.FastDBF
{

    /// <summary>
    /// This class represents a DBF IV file header.
    /// </summary>
    /// 
    /// <remarks>
    /// DBF files are really wasteful on space but this legacy format lives on because it's really really simple. 
    /// It lacks much in features though.
    /// 
    /// 
    /// Thanks to Erik Bachmann for providing the DBF file structure information!!
    /// http://www.clicketyclick.dk/databases/xbase/format/dbf.html
    /// 
    ///           _______________________  _______
    /// 00h /   0| Version number      *1|  ^
    ///          |-----------------------|  |
    /// 01h /   1| Date of last update   |  |
    /// 02h /   2|      YYMMDD        *21|  |
    /// 03h /   3|                    *14|  |
    ///          |-----------------------|  |
    /// 04h /   4| Number of records     | Record
    /// 05h /   5| in data file          | header
    /// 06h /   6| ( 32 bits )        *14|  |
    /// 07h /   7|                       |  |
    ///          |-----------------------|  |
    /// 08h /   8| Length of header   *14|  |
    /// 09h /   9| structure ( 16 bits ) |  |
    ///          |-----------------------|  |
    /// 0Ah /  10| Length of each record |  |
    /// 0Bh /  11| ( 16 bits )     *2 *14|  |
    ///          |-----------------------|  |
    /// 0Ch /  12| ( Reserved )        *3|  |
    /// 0Dh /  13|                       |  |
    ///          |-----------------------|  |
    /// 0Eh /  14| Incomplete transac.*12|  |
    ///          |-----------------------|  |
    /// 0Fh /  15| Encryption flag    *13|  |
    ///          |-----------------------|  |
    /// 10h /  16| Free record thread    |  |
    /// 11h /  17| (reserved for LAN     |  |
    /// 12h /  18|  only )               |  |
    /// 13h /  19|                       |  |
    ///          |-----------------------|  |
    /// 14h /  20| ( Reserved for        |  |            _        |=======================| ______
    ///          |   multi-user dBASE )  |  |           / 00h /  0| Field name in ASCII   |  ^
    ///          : ( dBASE III+ - )      :  |          /          : (terminated by 00h)   :  |
    ///          :                       :  |         |           |                       |  |
    /// 1Bh /  27|                       |  |         |   0Ah / 10|                       |  |
    ///          |-----------------------|  |         |           |-----------------------| For
    /// 1Ch /  28| MDX flag (dBASE IV)*14|  |         |   0Bh / 11| Field type (ASCII) *20| each
    ///          |-----------------------|  |         |           |-----------------------| field
    /// 1Dh /  29| Language driver     *5|  |        /    0Ch / 12| Field data address    |  |
    ///          |-----------------------|  |       /             |                     *6|  |
    /// 1Eh /  30| ( Reserved )          |  |      /              | (in memory !!!)       |  |
    /// 1Fh /  31|                     *3|  |     /       0Fh / 15| (dBASE III+)          |  |
    ///          |=======================|__|____/                |-----------------------|  |  -
    /// 20h /  32|                       |  |  ^          10h / 16| Field length       *22|  |   |
    ///          |- - - - - - - - - - - -|  |  |                  |-----------------------|  |   | *7
    ///          |                    *19|  |  |          11h / 17| Decimal count      *23|  |   |
    ///          |- - - - - - - - - - - -|  |  Field              |-----------------------|  |  -
    ///          |                       |  | Descriptor  12h / 18| ( Reserved for        |  |
    ///          :. . . . . . . . . . . .:  |  |array     13h / 19|   multi-user dBASE)*18|  |
    ///          :                       :  |  |                  |-----------------------|  |
    ///       n  |                       |__|__v_         14h / 20| Work area ID       *16|  |
    ///          |-----------------------|  |    \                |-----------------------|  |
    ///       n+1| Terminator (0Dh)      |  |     \       15h / 21| ( Reserved for        |  |
    ///          |=======================|  |      \      16h / 22|   multi-user dBASE )  |  |
    ///       m  | Database Container    |  |       \             |-----------------------|  |
    ///          :                    *15:  |        \    17h / 23| Flag for SET FIELDS   |  |
    ///          :                       :  |         |           |-----------------------|  |
    ///     / m+263                      |  |         |   18h / 24| ( Reserved )          |  |
    ///          |=======================|__v_ ___    |           :                       :  |
    ///          :                       :    ^       |           :                       :  |
    ///          :                       :    |       |           :                       :  |
    ///          :                       :    |       |   1Eh / 30|                       |  |
    ///          | Record structure      |    |       |           |-----------------------|  |
    ///          |                       |    |        \  1Fh / 31| Index field flag    *8|  |
    ///          |                       |    |         \_        |=======================| _v_____
    ///          |                       | Records
    ///          |-----------------------|    |
    ///          |                       |    |          _        |=======================| _______
    ///          |                       |    |         / 00h /  0| Record deleted flag *9|  ^
    ///          |                       |    |        /          |-----------------------|  |
    ///          |                       |    |       /           | Data               *10|  One
    ///          |                       |    |      /            : (ASCII)            *17: record
    ///          |                       |____|_____/             |                       |  |
    ///          :                       :    |                   |                       | _v_____
    ///          :                       :____|_____              |=======================|
    ///          :                       :    |
    ///          |                       |    |
    ///          |                       |    |
    ///          |                       |    |
    ///          |                       |    |
    ///          |                       |    |
    ///          |=======================|    |
    ///          |__End_of_File__________| ___v____  End of file ( 1Ah )  *11
    /// 
    /// </remarks>
    public class DbfHeader : ICloneable
    {

        /// <summary>
        /// Header file descriptor size is 33 bytes (32 bytes + 1 terminator byte), followed by column metadata which is 32 bytes each.
        /// </summary>
        public const int FileDescriptorSize = 33;


        /// <summary>
        /// Field or DBF Column descriptor is 32 bytes long.
        /// </summary>
        public const int ColumnDescriptorSize = 32;


        //type of the file, must be 03h
        private const int _fileType = 0x03;

        //Date the file was last updated.
        private DateTime _updateDate;

        //Number of records in the datafile, 32bit little-endian, unsigned 
        private uint _numRecords = 0;

        //Length of the header structure
        private ushort _headerLength = FileDescriptorSize;  //empty header is 33 bytes long. Each column adds 32 bytes.

        //Length of the records, ushort - unsigned 16 bit integer
        private int _recordLength = 1;  //start with 1 because the first byte is a delete flag

        //DBF fields/columns
        internal List<DbfColumn> _fields = new List<DbfColumn>();


        //indicates whether header columns can be modified!
        bool _locked = false;

        //keeps column name index for the header, must clear when header columns change.
        private Dictionary<string, int> _columnNameIndex = null;

        /// <summary>
        /// When object is modified dirty flag is set.
        /// </summary>
        bool _isDirty = false;


        /// <summary>
        /// mEmptyRecord is an array used to clear record data in CDbf4Record.
        /// This is shared by all record objects, used to speed up clearing fields or entire record.
        /// <seealso cref="EmptyDataRecord"/>
        /// </summary>
        private byte[] _emptyRecord = null;


        public readonly Encoding encoding = Encoding.ASCII;


        [Obsolete]
        public DbfHeader()
        {
        }

        public DbfHeader(Encoding encoding)
        {
            this.encoding = encoding;
        }


        /// <summary>
        /// Specify initial column capacity.
        /// </summary>
        /// <param name="nInitialFields"></param>
        public DbfHeader(int nFieldCapacity)
        {
            _fields = new List<DbfColumn>(nFieldCapacity);

        }


        /// <summary>
        /// Gets header length.
        /// </summary>
        public ushort HeaderLength
        {
            get
            {
                return _headerLength;
            }
        }


        /// <summary>
        /// Add a new column to the DBF header.
        /// </summary>
        /// <param name="oNewCol"></param>
        public void AddColumn(DbfColumn oNewCol)
        {

            //throw exception if the header is locked
            if (_locked)
                throw new InvalidOperationException("This header is locked and can not be modified. Modifying the header would result in a corrupt DBF file. You can unlock the header by calling UnLock() method.");

            //since we are breaking the spec rules about max number of fields, we should at least 
            //check that the record length stays within a number that can be recorded in the header!
            //we have 2 unsigned bytes for record length for a maximum of 65535.
            if (_recordLength + oNewCol.Length > 65535)
                throw new ArgumentOutOfRangeException("oNewCol", "Unable to add new column. Adding this column puts the record length over the maximum (which is 65535 bytes).");


            //add the column
            _fields.Add(oNewCol);

            //update offset bits, record and header lengths
            oNewCol._dataAddress = _recordLength;
            _recordLength += oNewCol.Length;
            _headerLength += ColumnDescriptorSize;

            //clear empty record
            _emptyRecord = null;

            //set dirty bit
            _isDirty = true;
            _columnNameIndex = null;

        }


        /// <summary>
        /// Create and add a new column with specified name and type.
        /// </summary>
        /// <param name="sName"></param>
        /// <param name="type"></param>
        public void AddColumn(string sName, DbfColumn.DbfColumnType type)
        {
            AddColumn(new DbfColumn(sName, type));
        }


        /// <summary>
        /// Create and add a new column with specified name, type, length, and decimal precision.
        /// </summary>
        /// <param name="sName">Field name. Uniqueness is not enforced.</param>
        /// <param name="type"></param>
        /// <param name="nLength">Length of the field including decimal point and decimal numbers</param>
        /// <param name="nDecimals">Number of decimal places to keep.</param>
        public void AddColumn(string sName, DbfColumn.DbfColumnType type, int nLength, int nDecimals)
        {
            AddColumn(new DbfColumn(sName, type, nLength, nDecimals));
        }


        /// <summary>
        /// Remove column from header definition.
        /// </summary>
        /// <param name="nIndex"></param>
        public void RemoveColumn(int nIndex)
        {
            //throw exception if the header is locked
            if (_locked)
                throw new InvalidOperationException("This header is locked and can not be modified. Modifying the header would result in a corrupt DBF file. You can unlock the header by calling UnLock() method.");


            DbfColumn oColRemove = _fields[nIndex];
            _fields.RemoveAt(nIndex);


            oColRemove._dataAddress = 0;
            _recordLength -= oColRemove.Length;
            _headerLength -= ColumnDescriptorSize;

            //if you remove a column offset shift for each of the columns 
            //following the one removed, we need to update those offsets.
            int nRemovedColLen = oColRemove.Length;
            for (int i = nIndex; i < _fields.Count; i++)
                _fields[i]._dataAddress -= nRemovedColLen;

            //clear the empty record
            _emptyRecord = null;

            //set dirty bit
            _isDirty = true;
            _columnNameIndex = null;

        }


        /// <summary>
        /// Look up a column index by name. NOT Case Sensitive. This is a change from previous behaviour!
        /// </summary>
        /// <param name="sName"></param>
        public DbfColumn this[string sName]
        {
            get
            {
                int colIndex = FindColumn(sName);
                if (colIndex > -1)
                    return _fields[colIndex];

                return null;

            }
        }


        /// <summary>
        /// Returns column at specified index. Index is 0 based.
        /// </summary>
        /// <param name="nIndex">Zero based index.</param>
        /// <returns></returns>
        public DbfColumn this[int nIndex]
        {
            get
            {
                return _fields[nIndex];
            }
        }


        /// <summary>
        /// Finds a column index by using a fast dictionary lookup-- creates column dictionary on first use. Returns -1 if not found. CHANGE: not case sensitive any longer!
        /// </summary>
        /// <param name="sName">Column name (case insensitive comparison)</param>
        /// <returns>column index (0 based) or -1 if not found.</returns>
        public int FindColumn(string sName)
        {

            if (_columnNameIndex == null)
            {
                _columnNameIndex = new Dictionary<string, int>(_fields.Count);

                //create a new index
                for (int i = 0; i < _fields.Count; i++)
                {
                    _columnNameIndex.Add(_fields[i].Name.ToUpper(), i);
                }
            }

            int columnIndex;
            if (_columnNameIndex.TryGetValue(sName.ToUpper(), out columnIndex))
                return columnIndex;

            return -1;

        }


        /// <summary>
        /// Returns an empty data record. This is used to clear columns 
        /// </summary>
        /// <remarks>
        /// The reason we put this in the header class is because it allows us to use the CDbf4Record class in two ways.
        /// 1. we can create one instance of the record and reuse it to write many records quickly clearing the data array by bitblting to it.
        /// 2. we can create many instances of the record (a collection of records) and have only one copy of this empty dataset for all of them.
        ///    If we had put it in the Record class then we would be taking up twice as much space unnecessarily. The empty record also fits the model
        ///    and everything is neatly encapsulated and safe.
        /// 
        /// </remarks>
        protected internal byte[] EmptyDataRecord
        {
            get { return _emptyRecord ?? (_emptyRecord = encoding.GetBytes("".PadLeft(_recordLength, ' ').ToCharArray())); }
        }


        /// <summary>
        /// Returns Number of columns in this dbf header.
        /// </summary>
        public int ColumnCount
        {
            get { return _fields.Count; }

        }


        /// <summary>
        /// Size of one record in bytes. All fields + 1 byte delete flag.
        /// </summary>
        public int RecordLength
        {
            get
            {
                return _recordLength;
            }
        }


        /// <summary>
        /// Get/Set number of records in the DBF.
        /// </summary>
        /// <remarks>
        /// The reason we allow client to set RecordCount is beause in certain streams 
        /// like internet streams we can not update record count as we write out records, we have to set it in advance,
        /// so client has to be able to modify this property.
        /// </remarks>
        public uint RecordCount
        {
            get
            {
                return _numRecords;
            }

            set
            {
                _numRecords = value;

                //set the dirty bit
                _isDirty = true;

            }
        }


        /// <summary>
        /// Get/set whether this header is read only or can be modified. When you create a CDbfRecord 
        /// object and pass a header to it, CDbfRecord locks the header so that it can not be modified any longer.
        /// in order to preserve DBF integrity.
        /// </summary>
        internal bool Locked
        {
            get
            {
                return _locked;
            }

            set
            {
                _locked = value;
            }

        }


        /// <summary>
        /// Use this method with caution. Headers are locked for a reason, to prevent DBF from becoming corrupt.
        /// </summary>
        public void Unlock()
        {
            _locked = false;
        }


        /// <summary>
        /// Returns true when this object is modified after read or write.
        /// </summary>
        public bool IsDirty
        {
            get
            {
                return _isDirty;
            }

            set
            {
                _isDirty = value;
            }
        }


        /// <summary>
        /// Encoding must be ASCII for this binary writer.
        /// </summary>
        /// <param name="writer"></param>
        /// <remarks>
        /// See class remarks for DBF file structure.
        /// </remarks>
        public void Write(BinaryWriter writer)
        {

            //write the header
            // write the output file type.
            writer.Write((byte)_fileType);

            //Update date format is YYMMDD, which is different from the column Date type (YYYYDDMM)
            writer.Write((byte)(_updateDate.Year - 1900));
            writer.Write((byte)_updateDate.Month);
            writer.Write((byte)_updateDate.Day);

            // write the number of records in the datafile. (32 bit number, little-endian unsigned)
            writer.Write(_numRecords);

            // write the length of the header structure.
            writer.Write(_headerLength);

            // write the length of a record
            writer.Write((ushort)_recordLength);

            // write the reserved bytes in the header
            for (int i = 0; i < 20; i++)
                writer.Write((byte)0);

            // write all of the header records
            byte[] byteReserved = new byte[14];  //these are initialized to 0 by default.
            foreach (DbfColumn field in _fields)
            {
                char[] cname = field.Name.PadRight(11, (char)0).ToCharArray();
                writer.Write(cname);

                // write the field type
                writer.Write((char)field.ColumnTypeChar);

                // write the field data address, offset from the start of the record.
                writer.Write(field.DataAddress);


                // write the length of the field.
                // if char field is longer than 255 bytes, then we use the decimal field as part of the field length.
                if (field.ColumnType == DbfColumn.DbfColumnType.Character && field.Length > 255)
                {
                    //treat decimal count as high byte of field length, this extends char field max to 65535
                    writer.Write((ushort)field.Length);

                }
                else
                {
                    // write the length of the field.
                    writer.Write((byte)field.Length);

                    // write the decimal count.
                    writer.Write((byte)field.DecimalCount);
                }

                // write the reserved bytes.
                writer.Write(byteReserved);

            }

            // write the end of the field definitions marker
            writer.Write((byte)0x0D);
            writer.Flush();

            //clear dirty bit
            _isDirty = false;


            //lock the header so it can not be modified any longer, 
            //we could actually postpond this until first record is written!
            _locked = true;


        }


        /// <summary>
        /// Read header data, make sure the stream is positioned at the start of the file to read the header otherwise you will get an exception.
        /// When this function is done the position will be the first record.
        /// </summary>
        /// <param name="reader"></param>
        public void Read(BinaryReader reader)
        {

            // type of reader.
            int nFileType = reader.ReadByte();

            if (nFileType != 0x03)
                throw new NotSupportedException("Unsupported DBF reader Type " + nFileType);

            // parse the update date information.
            int year = (int)reader.ReadByte();
            int month = (int)reader.ReadByte();
            int day = (int)reader.ReadByte();
            _updateDate = new DateTime(year + 1900, month, day);

            // read the number of records.
            _numRecords = reader.ReadUInt32();

            // read the length of the header structure.
            _headerLength = reader.ReadUInt16();

            // read the length of a record
            _recordLength = reader.ReadInt16();

            // skip the reserved bytes in the header.
            reader.ReadBytes(20);

            // calculate the number of Fields in the header
            int nNumFields = (_headerLength - FileDescriptorSize) / ColumnDescriptorSize;

            //offset from start of record, start at 1 because that's the delete flag.
            int nDataOffset = 1;

            // read all of the header records
            _fields = new List<DbfColumn>(nNumFields);
            for (int i = 0; i < nNumFields; i++)
            {

                // read the field name				
                char[] buffer = new char[11];
                buffer = reader.ReadChars(11);
                string sFieldName = new string(buffer);
                int nullPoint = sFieldName.IndexOf((char)0);
                if (nullPoint != -1)
                    sFieldName = sFieldName.Substring(0, nullPoint);


                //read the field type
                char cDbaseType = (char)reader.ReadByte();

                // read the field data address, offset from the start of the record.
                int nFieldDataAddress = reader.ReadInt32();


                //read the field length in bytes
                //if field type is char, then read FieldLength and Decimal count as one number to allow char fields to be
                //longer than 256 bytes (ASCII char). This is the way Clipper and FoxPro do it, and there is really no downside
                //since for char fields decimal count should be zero for other versions that do not support this extended functionality.
                //-----------------------------------------------------------------------------------------------------------------------
                int nFieldLength = 0;
                int nDecimals = 0;
                if (cDbaseType == 'C' || cDbaseType == 'c')
                {
                    //treat decimal count as high byte
                    nFieldLength = (int)reader.ReadUInt16();
                }
                else
                {
                    //read field length as an unsigned byte.
                    nFieldLength = (int)reader.ReadByte();

                    //read decimal count as one byte
                    nDecimals = (int)reader.ReadByte();

                }


                //read the reserved bytes.
                reader.ReadBytes(14);

                //Create and add field to collection
                _fields.Add(new DbfColumn(sFieldName, DbfColumn.GetDbaseType(cDbaseType), nFieldLength, nDecimals, nDataOffset));

                // add up address information, you can not trust the address recorded in the DBF file...
                nDataOffset += nFieldLength;

            }

            // Last byte is a marker for the end of the field definitions.
            reader.ReadBytes(1);


            //read any extra header bytes...move to first record
            //equivalent to reader.BaseStream.Seek(mHeaderLength, SeekOrigin.Begin) except that we are not using the seek function since
            //we need to support streams that can not seek like web connections.
            int nExtraReadBytes = _headerLength - (FileDescriptorSize + (ColumnDescriptorSize * _fields.Count));
            if (nExtraReadBytes > 0)
                reader.ReadBytes(nExtraReadBytes);



            //if the stream is not forward-only, calculate number of records using file size, 
            //sometimes the header does not contain the correct record count
            //if we are reading the file from the web, we have to use ReadNext() functions anyway so
            //Number of records is not so important and we can trust the DBF to have it stored correctly.
            if (reader.BaseStream.CanSeek && _numRecords == 0)
            {
                //notice here that we subtract file end byte which is supposed to be 0x1A,
                //but some DBF files are incorrectly written without this byte, so we round off to nearest integer.
                //that gives a correct result with or without ending byte.
                if (_recordLength > 0)
                    _numRecords = (uint)Math.Round(((double)(reader.BaseStream.Length - _headerLength - 1) / _recordLength));

            }


            //lock header since it was read from a file. we don't want it modified because that would corrupt the file.
            //user can override this lock if really necessary by calling UnLock() method.
            _locked = true;

            //clear dirty bit
            _isDirty = false;

        }



        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
