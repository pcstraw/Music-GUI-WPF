using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace Glaxion.Music
{
    public class Database
    {
        public static Database Instance { get { return Nested.instance; } }
        private class Nested
        {
            static Nested() { }//lazy singleton
            internal static readonly Database instance = new Database();
        }

        string musicDirectoryTable = "MusicDirectoryTable";
        string playlistDirectoryTable = "PlaylistDirectoryTable";

        SQLiteConnection OpenConnection()
        {
            string cs = "Data Source=database.sqlite;Version=3;";
            return new SQLiteConnection(cs);
        }
        
        public void DeleteMusicDirectory(string directory)
        {
            DeleteDirectory(directory, musicDirectoryTable);
        }

        public void DeletePlaylistDirectory(string directory)
        {
            DeleteDirectory(directory, playlistDirectoryTable);
        }

        public void DeleteDirectory(string directory,string directoryTable)
        {
            Tools.tool.debug("Deleting directory " + directory + " from database: " + directoryTable);

            using (SQLiteConnection connection = OpenConnection())
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                    connection.Open();

                SQLiteDataAdapter adapter = new SQLiteDataAdapter();
                string _directory = "'" + directory.Replace("'", "''") + "'";
                string dropDirectoryTableCommand = "DROP TABLE IF EXISTS " + _directory;
                SQLiteCommand tableCommand = new SQLiteCommand(dropDirectoryTableCommand, connection);
                tableCommand.ExecuteNonQuery();

                string sql = "DELETE FROM " + directoryTable + " WHERE DirectoryID = " + _directory;
                SQLiteCommand deleteCommand = new SQLiteCommand(sql, connection);
                deleteCommand.ExecuteNonQuery();
            }
        }

        //https://stackoverflow.com/questions/19732353/how-to-drop-sqlite-table-after-it-already-exists-in-c-sharp
        public void PopulateMusicFiles(FileDirectory fileDirectory)
        {
            Tools.tool.debug("Populating Database with music files ");
            PopulateWithDirectory(fileDirectory, musicDirectoryTable);
        }
        
        //https://stackoverflow.com/questions/19732353/how-to-drop-sqlite-table-after-it-already-exists-in-c-sharp
        public void PopulatePlaylistFiles(FileDirectory fileDirectory)
        {
            PopulateWithDirectory(fileDirectory, playlistDirectoryTable);
        }

        //https://stackoverflow.com/questions/19732353/how-to-drop-sqlite-table-after-it-already-exists-in-c-sharp
        public void PopulateWithDirectory(FileDirectory fileDirectory,string directoryTableName)
        {
            Tools.tool.debug("Populating Database : Directory: " + fileDirectory + "Database Table Name: " + directoryTableName);

            string _directoryTableName = directoryTableName;

            using (SQLiteConnection connection = OpenConnection())
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                    connection.Open();

                SQLiteDataAdapter adapter = new SQLiteDataAdapter();
                string createFileTableCommand = "CREATE TABLE IF NOT EXISTS " + _directoryTableName + " (DirectoryID TEXT PRIMARY KEY) WITHOUT ROWID";
                SQLiteCommand tableCommand = new SQLiteCommand(createFileTableCommand, connection);
                tableCommand.ExecuteNonQuery();

                string _directory = "'" + fileDirectory.directory.Replace("'", "''") + "'";
                string sql = "INSERT OR IGNORE INTO " + _directoryTableName + " (DirectoryID) VALUES(" + _directory + ")";
                adapter.InsertCommand = new SQLiteCommand(sql, connection);
                adapter.InsertCommand.ExecuteNonQuery();

                createFileTableCommand = "CREATE TABLE IF NOT EXISTS " + _directory + " (FilepathID TEXT PRIMARY KEY) WITHOUT ROWID";
                tableCommand = new SQLiteCommand(createFileTableCommand, connection);
                tableCommand.ExecuteNonQuery();
                adapter.InsertCommand = new SQLiteCommand();

                SQLiteCommand insertCommand = connection.CreateCommand();
                SQLiteTransaction transaction = connection.BeginTransaction();
                insertCommand.CommandText = "INSERT OR IGNORE INTO " + _directory + " (FilepathID) VALUES(@FilepathID)";
                insertCommand.Parameters.AddWithValue("FilepathID", "");

                foreach (string file in fileDirectory)
                {
                    string _file = file.Replace("'", "''");
                    insertCommand.Parameters["FilepathID"].Value = "'" + _file + "'";
                    insertCommand.ExecuteNonQuery();
                }
                transaction.Commit();
                insertCommand.Dispose();
            }
        }

        public void RetreiveMusicFiles()
        {
            List<FileDirectory> fileDirectories = RetreiveDirectoryFiles(musicDirectoryTable);
            MusicFileManager.Instance.Directories.AddRange(fileDirectories);
        }

        public void RetreivePlaylistFiles()
        {
            List<FileDirectory> playlistDirectories = RetreiveDirectoryFiles(playlistDirectoryTable);
            PlaylistFileManager.Instance.Directories.AddRange(playlistDirectories);
        }

        public List<FileDirectory> RetreiveDirectoryFiles(string directoryTableName)
        {
            Tools.tool.debug("Retreiving Directory Files: " + directoryTableName);
            string _directoryTablename = directoryTableName;
            List<FileDirectory> fileDirectories = new List<FileDirectory>();

            using (SQLiteConnection connection = OpenConnection())
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                    connection.Open();

                string queryDirectorypaths = "SELECT * FROM " + _directoryTablename;
                SQLiteCommand dirReadCommnad = connection.CreateCommand();
                dirReadCommnad.CommandText = queryDirectorypaths;
                try
                {
                    SQLiteDataReader directoryReader = dirReadCommnad.ExecuteReader();
                    while (directoryReader.Read())
                    {
                        string directoryPath = (string)directoryReader["DirectoryID"];
                        string _directory = "'" + directoryPath.Replace("'", "''") + "'";
                        Console.WriteLine("Retreiving Directory: " + directoryPath);
                        FileDirectory fileDirectory = new FileDirectory();
                        fileDirectory.directory = directoryPath;

                        string queryFilepaths = "SELECT * FROM " + _directory;
                        SQLiteCommand readCommnad = connection.CreateCommand();
                        readCommnad.CommandText = queryFilepaths;
                        SQLiteDataReader dataReader = readCommnad.ExecuteReader();

                        while (dataReader.Read())
                        {
                            string filepath = (string)dataReader["FilepathID"];

                            if (filepath == "")
                                continue;
                            filepath = filepath.Trim('\'');
                            string f = filepath.Replace("''", "'");
                            fileDirectory.Add(f);
                            //Console.WriteLine("->>" + filepath);
                        }
                        fileDirectories.Add(fileDirectory);
                    }
                }catch { }
            }
            return fileDirectories;
        }
    }
}
