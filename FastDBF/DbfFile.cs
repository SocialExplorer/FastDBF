///
/// Author: Ahmed Lacevic
/// Date: 12/1/2007
/// Desc: This class represents a DBF file. You can create, open, update and save DBF files using this class and supporting classes.
/// Also, this class supports reading/writing from/to an internet forward only type of stream!
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
    /// This class represents a DBF file. You can create new, open, update and save DBF files using this class and supporting classes.
    /// Also, this class supports reading/writing from/to an internet forward only type of stream!
    /// </summary>
    /// <remarks>
    /// TODO: add end of file byte '0x1A' !!!
    /// We don't relly on that byte at all, and everything works with or without that byte, but it should be there by spec.
    /// </remarks>
    public class DbfFile
    {

        /// <summary>
        /// Helps read/write dbf file header information.
        /// </summary>
        protected DbfHeader _header;


        /// <summary>
        /// flag that indicates whether the header was written or not...
        /// </summary>
        protected bool _headerWritten = false;


        /// <summary>
        /// Streams to read and write to the DBF file.
        /// </summary>
        protected Stream _dbfFile = null;
        protected BinaryReader _dbfFileReader = null;
        protected BinaryWriter _dbfFileWriter = null;

        /// <summary>
        /// By default use windows 1252 code page encoding.
        /// </summary>
        private Encoding encoding =  Encoding.GetEncoding(1252);

        /// <summary>
        /// File that was opened, if one was opened at all.
        /// </summary>
        protected string _fileName = "";


        /// <summary>
        /// Number of records read using ReadNext() methods only. This applies only when we are using a forward-only stream.
        /// mRecordsReadCount is used to keep track of record index. With a seek enabled stream, 
        /// we can always calculate index using stream position.
        /// </summary>
        protected long _recordsReadCount = 0;


        /// <summary>
        /// keep these values handy so we don't call functions on every read.
        /// </summary>
        protected bool _isForwardOnly = false;
        protected bool _isReadOnly = false;


        [Obsolete]
        public DbfFile()
            : this(Encoding.GetEncoding(1252))
        {
        }

        public DbfFile(Encoding encoding)
        {
            this.encoding = encoding;
            _header = new DbfHeader(encoding);
        }

        /// <summary>
        /// Open a DBF from a FileStream. This can be a file or an internet connection stream. Make sure that it is positioned at start of DBF file.
        /// Reading a DBF over the internet we can not determine size of the file, so we support HasMore(), ReadNext() interface. 
        /// RecordCount information in header can not be trusted always, since some packages store 0 there.
        /// </summary>
        /// <param name="ofs"></param>
        public void Open(Stream ofs)
        {
            if (_dbfFile != null)
                Close();

            _dbfFile = ofs;
            _dbfFileReader = null;
            _dbfFileWriter = null;

            if (_dbfFile.CanRead)
                _dbfFileReader = new BinaryReader(_dbfFile, encoding);

            if (_dbfFile.CanWrite)
                _dbfFileWriter = new BinaryWriter(_dbfFile, encoding);

            //reset position
            _recordsReadCount = 0;

            //assume header is not written
            _headerWritten = false;

            //read the header
            if (ofs.CanRead)
            {
                //try to read the header...
                try
                {
                    _header.Read(_dbfFileReader);
                    _headerWritten = true;

                }
                catch (EndOfStreamException)
                {
                    //could not read header, file is empty
                    _header = new DbfHeader(encoding);
                    _headerWritten = false;
                }


            }

            if (_dbfFile != null)
            {
                _isReadOnly = !_dbfFile.CanWrite;
                _isForwardOnly = !_dbfFile.CanSeek;
            }


        }



        /// <summary>
        /// Open a DBF file or create a new one.
        /// </summary>
        /// <param name="sPath">Full path to the file.</param>
        /// <param name="mode"></param>
        public void Open(string sPath, FileMode mode, FileAccess access, FileShare share)
        {
            _fileName = sPath;
            Open(File.Open(sPath, mode, access, share));
        }

        /// <summary>
        /// Open a DBF file or create a new one.
        /// </summary>
        /// <param name="sPath">Full path to the file.</param>
        /// <param name="mode"></param>
        public void Open(string sPath, FileMode mode, FileAccess access)
        {
            _fileName = sPath;
            Open(File.Open(sPath, mode, access));
        }

        /// <summary>
        /// Open a DBF file or create a new one.
        /// </summary>
        /// <param name="sPath">Full path to the file.</param>
        /// <param name="mode"></param>
        public void Open(string sPath, FileMode mode)
        {
            _fileName = sPath;
            Open(File.Open(sPath, mode));
        }


        /// <summary>
        /// Creates a new DBF 4 file. Overwrites if file exists! Use Open() function for more options.
        /// </summary>
        /// <param name="sPath"></param>
        public void Create(string sPath)
        {
            Open(sPath, FileMode.Create, FileAccess.ReadWrite);
            _headerWritten = false;

        }



        /// <summary>
        /// Update header info, flush buffers and close streams. You should always call this method when you are done with a DBF file.
        /// </summary>
        public void Close()
        {

            //try to update the header if it has changed
            //------------------------------------------
            if (_header.IsDirty)
                WriteHeader();



            //Empty header...
            //--------------------------------
            _header = new DbfHeader(encoding);
            _headerWritten = false;


            //reset current record index
            //--------------------------------
            _recordsReadCount = 0;


            //Close streams...
            //--------------------------------
            if (_dbfFileWriter != null)
            {
                _dbfFileWriter.Flush();
                _dbfFileWriter.Close();
            }

            if (_dbfFileReader != null)
                _dbfFileReader.Close();

            if (_dbfFile != null)
            {
                _dbfFile.Close();
                _dbfFile.Dispose();
            }


            //set streams to null
            //--------------------------------
            _dbfFileReader = null;
            _dbfFileWriter = null;
            _dbfFile = null;

            _fileName = "";

        }



        /// <summary>
        /// Returns true if we can not write to the DBF file stream.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return _isReadOnly;
                /*
                if (mDbfFile != null)
                  return !mDbfFile.CanWrite; 
                return true;
                */

            }

        }


        /// <summary>
        /// Returns true if we can not seek to different locations within the file, such as internet connections.
        /// </summary>
        public bool IsForwardOnly
        {
            get
            {
                return _isForwardOnly;
                /*
                if(mDbfFile!=null)
                  return !mDbfFile.CanSeek;
        
                return false;
                */
            }
        }


        /// <summary>
        /// Returns the name of the filestream.
        /// </summary>
        public string FileName
        {
            get
            {
                return _fileName;
            }
        }



        /// <summary>
        /// Read next record and fill data into parameter oFillRecord. Returns true if a record was read, otherwise false.
        /// </summary>
        /// <param name="oFillRecord"></param>
        /// <returns></returns>
        public bool ReadNext(DbfRecord oFillRecord)
        {

            //check if we can fill this record with data. it must match record size specified by header and number of columns.
            //we are not checking whether it comes from another DBF file or not, we just need the same structure. Allow flexibility but be safe.
            if (oFillRecord.Header != _header && (oFillRecord.Header.ColumnCount != _header.ColumnCount || oFillRecord.Header.RecordLength != _header.RecordLength))
                throw new Exception("Record parameter does not have the same size and number of columns as the " +
                                    "header specifies, so we are unable to read a record into oFillRecord. " +
                                    "This is a programming error, have you mixed up DBF file objects?");

            //DBF file reader can be null if stream is not readable...
            if (_dbfFileReader == null)
                throw new Exception("Read stream is null, either you have opened a stream that can not be " +
                                    "read from (a write-only stream) or you have not opened a stream at all.");

            //read next record...
            bool bRead = oFillRecord.Read(_dbfFile);

            if (bRead)
            {
                if (_isForwardOnly)
                {
                    //zero based index! set before incrementing count.
                    oFillRecord.RecordIndex = _recordsReadCount;
                    _recordsReadCount++;
                }
                else
                    oFillRecord.RecordIndex = ((int)((_dbfFile.Position - _header.HeaderLength) / _header.RecordLength)) - 1;

            }

            return bRead;

        }


        /// <summary>
        /// Tries to read a record and returns a new record object or null if nothing was read.
        /// </summary>
        /// <returns></returns>
        public DbfRecord ReadNext()
        {
            //create a new record and fill it.
            DbfRecord orec = new DbfRecord(_header);

            return ReadNext(orec) ? orec : null;

        }



        /// <summary>
        /// Reads a record specified by index into oFillRecord object. You can use this method 
        /// to read in and process records without creating and discarding record objects.
        /// Note that you should check that your stream is not forward-only! If you have a forward only stream, use ReadNext() functions.
        /// </summary>
        /// <param name="index">Zero based record index.</param>
        /// <param name="oFillRecord">Record object to fill, must have same size and number of fields as thid DBF file header!</param>
        /// <remarks>
        /// <returns>True if read a record was read, otherwise false. If you read end of file false will be returned and oFillRecord will NOT be modified!</returns>
        /// The parameter record (oFillRecord) must match record size specified by the header and number of columns as well.
        /// It does not have to come from the same header, but it must match the structure. We are not going as far as to check size of each field.
        /// The idea is to be flexible but safe. It's a fine balance, these two are almost always at odds.
        /// </remarks>
        public bool Read(long index, DbfRecord oFillRecord)
        {

            //check if we can fill this record with data. it must match record size specified by header and number of columns.
            //we are not checking whether it comes from another DBF file or not, we just need the same structure. Allow flexibility but be safe.
            if (oFillRecord.Header != _header && (oFillRecord.Header.ColumnCount != _header.ColumnCount || oFillRecord.Header.RecordLength != _header.RecordLength))
                throw new Exception("Record parameter does not have the same size and number of columns as the " +
                                    "header specifies, so we are unable to read a record into oFillRecord. " +
                                    "This is a programming error, have you mixed up DBF file objects?");

            //DBF file reader can be null if stream is not readable...
            if (_dbfFileReader == null)
                throw new Exception("ReadStream is null, either you have opened a stream that can not be " +
                                    "read from (a write-only stream) or you have not opened a stream at all.");


            //move to the specified record, note that an exception will be thrown is stream is not seekable! 
            //This is ok, since we provide a function to check whether the stream is seekable. 
            long nSeekToPosition = _header.HeaderLength + (index * _header.RecordLength);

            //check whether requested record exists. Subtract 1 from file length (there is a terminating character 1A at the end of the file)
            //so if we hit end of file, there are no more records, so return false;
            if (index < 0 || _dbfFile.Length - 1 <= nSeekToPosition)
                return false;

            //move to record and read
            _dbfFile.Seek(nSeekToPosition, SeekOrigin.Begin);

            //read the record
            bool bRead = oFillRecord.Read(_dbfFile);
            if (bRead)
                oFillRecord.RecordIndex = index;

            return bRead;

        }

        public bool ReadValue(int rowIndex, int columnIndex, out string result)
        {

            result = String.Empty;

            DbfColumn ocol = _header[columnIndex];

            //move to the specified record, note that an exception will be thrown is stream is not seekable! 
            //This is ok, since we provide a function to check whether the stream is seekable. 
            long nSeekToPosition = _header.HeaderLength + (rowIndex * _header.RecordLength) + ocol.DataAddress;

            //check whether requested record exists. Subtract 1 from file length (there is a terminating character 1A at the end of the file)
            //so if we hit end of file, there are no more records, so return false;
            if (rowIndex < 0 || _dbfFile.Length - 1 <= nSeekToPosition)
                return false;

            //move to position and read
            _dbfFile.Seek(nSeekToPosition, SeekOrigin.Begin);

            //read the value
            byte[] data = new byte[ocol.Length];
            _dbfFile.Read(data, 0, ocol.Length);
            result = new string(encoding.GetChars(data, 0, ocol.Length));

            return true;
        }

        /// <summary>
        /// Reads a record specified by index. This method requires the stream to be able to seek to position. 
        /// If you are using a http stream, or a stream that can not stream, use ReadNext() methods to read in all records.
        /// </summary>
        /// <param name="index">Zero based index.</param>
        /// <returns>Null if record can not be read, otherwise returns a new record.</returns>
        public DbfRecord Read(long index)
        {
            //create a new record and fill it.
            DbfRecord orec = new DbfRecord(_header);

            return Read(index, orec) ? orec : null;

        }




        /// <summary>
        /// Write a record to file. If RecordIndex is present, record will be updated, otherwise a new record will be written.
        /// Header will be output first if this is the first record being writen to file. 
        /// This method does not require stream seek capability to add a new record.
        /// </summary>
        /// <param name="orec"></param>
        public void Write(DbfRecord orec)
        {

            //if header was never written, write it first, then output the record
            if (!_headerWritten)
                WriteHeader();

            //if this is a new record (RecordIndex should be -1 in that case)
            if (orec.RecordIndex < 0)
            {
                if (_dbfFileWriter.BaseStream.CanSeek)
                {
                    //calculate number of records in file. do not rely on header's RecordCount property since client can change that value.
                    //also note that some DBF files do not have ending 0x1A byte, so we subtract 1 and round off 
                    //instead of just cast since cast would just drop decimals.
                    int nNumRecords = (int)Math.Round(((double)(_dbfFile.Length - _header.HeaderLength - 1) / _header.RecordLength));
                    if (nNumRecords < 0)
                        nNumRecords = 0;

                    orec.RecordIndex = nNumRecords;
                    Update(orec);
                    _header.RecordCount++;

                }
                else
                {
                    //we can not position this stream, just write out the new record.
                    orec.Write(_dbfFile);
                    _header.RecordCount++;
                }
            }
            else
                Update(orec);

        }

        public void Write(DbfRecord orec, bool bClearRecordAfterWrite)
        {

            Write(orec);

            if (bClearRecordAfterWrite)
                orec.Clear();

        }


        /// <summary>
        /// Update a record. RecordIndex (zero based index) must be more than -1, otherwise an exception is thrown.
        /// You can also use Write method which updates a record if it has RecordIndex or adds a new one if RecordIndex == -1.
        /// RecordIndex is set automatically when you call any Read() methods on this class.
        /// </summary>
        /// <param name="orec"></param>
        public void Update(DbfRecord orec)
        {

            //if header was never written, write it first, then output the record
            if (!_headerWritten)
                WriteHeader();


            //Check if record has an index
            if (orec.RecordIndex < 0)
                throw new Exception("RecordIndex is not set, unable to update record. Set RecordIndex or call Write() method to add a new record to file.");


            //Check if this record matches record size specified by header and number of columns. 
            //Client can pass a record from another DBF that is incompatible with this one and that would corrupt the file.
            if (orec.Header != _header && (orec.Header.ColumnCount != _header.ColumnCount || orec.Header.RecordLength != _header.RecordLength))
                throw new Exception("Record parameter does not have the same size and number of columns as the " +
                                    "header specifies. Writing this record would corrupt the DBF file. " +
                                    "This is a programming error, have you mixed up DBF file objects?");

            //DBF file writer can be null if stream is not writable to...
            if (_dbfFileWriter == null)
                throw new Exception("Write stream is null. Either you have opened a stream that can not be " +
                                    "writen to (a read-only stream) or you have not opened a stream at all.");


            //move to the specified record, note that an exception will be thrown if stream is not seekable! 
            //This is ok, since we provide a function to check whether the stream is seekable. 
            long nSeekToPosition = (long)_header.HeaderLength + (long)((long)orec.RecordIndex * (long)_header.RecordLength);

            //check whether we can seek to this position. Subtract 1 from file length (there is a terminating character 1A at the end of the file)
            //so if we hit end of file, there are no more records, so return false;
            if (_dbfFile.Length < nSeekToPosition)
                throw new Exception("Invalid record position. Unable to save record.");

            //move to record start
            _dbfFile.Seek(nSeekToPosition, SeekOrigin.Begin);

            //write
            orec.Write(_dbfFile);


        }



        /// <summary>
        /// Save header to file. Normally, you do not have to call this method, header is saved 
        /// automatically and updated when you close the file (if it changed).
        /// </summary>
        public bool WriteHeader()
        {

            //update header if possible
            //--------------------------------
            if (_dbfFileWriter != null)
            {
                if (_dbfFileWriter.BaseStream.CanSeek)
                {
                    _dbfFileWriter.Seek(0, SeekOrigin.Begin);
                    _header.Write(_dbfFileWriter);
                    _headerWritten = true;
                    return true;
                }
                else
                {
                    //if stream can not seek, then just write it out and that's it.
                    if (!_headerWritten)
                        _header.Write(_dbfFileWriter);

                    _headerWritten = true;

                }
            }

            return false;

        }



        /// <summary>
        /// Access DBF header with information on columns. Use this object for faster access to header. 
        /// Remove one layer of function calls by saving header reference and using it directly to access columns.
        /// </summary>
        public DbfHeader Header
        {
            get
            {
                return _header;
            }
        }



    }
}
