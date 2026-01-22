using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;


namespace CrowRx.Utility
{
    public class Ftp
    {
        private const int BufferSize = 2048;

        private readonly string _hostIP;
        private readonly string _userName;
        private readonly string _password;

        private FtpWebRequest _ftpRequest;


        public Ftp(string hostIP, string userName, string password)
        {
            _hostIP = hostIP;
            _userName = userName;
            _password = password;
        }


        /// <summary>
        /// Download File
        /// </summary>
        /// <param name="remoteFile">file path in ftp server</param>
        /// <param name="localFile">file path for save in local</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> Download(string remoteFile, string localFile, CancellationToken cancellationToken)
        {
            bool isSuccess;

            try
            {
                /* Create an FTP Request */
                _ftpRequest = (FtpWebRequest)WebRequest.Create(_hostIP + "/" + remoteFile);

                /* Log in to the FTP Server with the User Name and Password Provided */
                _ftpRequest.Credentials = new NetworkCredential(_userName, _password);

                /* When in doubt, use these options */
                _ftpRequest.UseBinary = true;
                _ftpRequest.UsePassive = true;
                _ftpRequest.KeepAlive = true;

                /* Specify the Type of FTP Request */
                _ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                /* Establish Return Communication with the FTP Server */
                using var ftpResponse = (FtpWebResponse)await _ftpRequest.GetResponseAsync();

                /* Get the FTP Server's Response Stream */
                await using var ftpStream = ftpResponse.GetResponseStream();

                if (ftpStream is null)
                {
                    isSuccess = false;
                }
                else
                {
                    /* Open a File Stream to Write the Downloaded File */
                    await using var localFileStream = new FileStream(localFile, FileMode.Create);

                    /* Buffer for the Downloaded Data */
                    var byteBuffer = new byte[BufferSize];
                    int bytesRead = await ftpStream.ReadAsync(byteBuffer, 0, BufferSize, cancellationToken);

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        /* Download the File by Writing the Buffered Data Until the Transfer is Complete */
                        try
                        {
                            while (bytesRead > 0)
                            {
                                await localFileStream.WriteAsync(byteBuffer, 0, bytesRead, cancellationToken);

                                if (cancellationToken.IsCancellationRequested)
                                {
                                    break;
                                }

                                bytesRead = await ftpStream.ReadAsync(byteBuffer, 0, BufferSize, cancellationToken);

                                if (cancellationToken.IsCancellationRequested)
                                {
                                    break;
                                }
                            }

                            isSuccess = !cancellationToken.IsCancellationRequested;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());

                            isSuccess = false;
                        }
                    }
                    else
                    {
                        isSuccess = false;
                    }
                }

                _ftpRequest = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                isSuccess = false;
            }

            return isSuccess;
        }

        /// <summary>
        /// Upload File
        /// </summary>
        /// <param name="remoteFile"></param>
        /// <param name="localFile"></param>
        /// <param name="cancellationToken"></param>
        public async Task<bool> Upload(string remoteFile, string localFile, CancellationToken cancellationToken)
        {
            bool isSuccess;

            try
            {
                /* Create an FTP Request */
                _ftpRequest = (FtpWebRequest)WebRequest.Create(_hostIP + "/" + remoteFile);

                /* Log in to the FTP Server with the User Name and Password Provided */
                _ftpRequest.Credentials = new NetworkCredential(_userName, _password);

                /* When in doubt, use these options */
                _ftpRequest.UseBinary = true;
                _ftpRequest.UsePassive = true;
                _ftpRequest.KeepAlive = true;

                /* Specify the Type of FTP Request */
                _ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

                /* Establish Return Communication with the FTP Server */
                await using var ftpStream = await _ftpRequest.GetRequestStreamAsync();

                /* Open a File Stream to Read the File for Upload */
                await using var localFileStream = new FileStream(localFile, FileMode.Open);

                /* Buffer for the Downloaded Data */
                var byteBuffer = new byte[BufferSize];
                int bytesSent = await localFileStream.ReadAsync(byteBuffer, 0, BufferSize, cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    /* Upload the File by Sending the Buffered Data Until the Transfer is Complete */
                    try
                    {
                        while (bytesSent != 0)
                        {
                            await ftpStream.WriteAsync(byteBuffer, 0, bytesSent, cancellationToken);

                            if (cancellationToken.IsCancellationRequested)
                            {
                                break;
                            }

                            bytesSent = await localFileStream.ReadAsync(byteBuffer, 0, BufferSize, cancellationToken);

                            if (cancellationToken.IsCancellationRequested)
                            {
                                break;
                            }
                        }

                        isSuccess = !cancellationToken.IsCancellationRequested;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());

                        isSuccess = false;
                    }
                }
                else
                {
                    isSuccess = false;
                }

                _ftpRequest = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                isSuccess = false;
            }

            return isSuccess;
        }

        /// <summary>
        /// Delete File
        /// </summary>
        /// <param name="deleteFile"></param>
        public async void Delete(string deleteFile)
        {
            try
            {
                /* Create an FTP Request */
                _ftpRequest = (FtpWebRequest)WebRequest.Create(_hostIP + "/" + deleteFile);

                /* Log in to the FTP Server with the User Name and Password Provided */
                _ftpRequest.Credentials = new NetworkCredential(_userName, _password);

                /* When in doubt, use these options */
                _ftpRequest.UseBinary = true;
                _ftpRequest.UsePassive = true;
                _ftpRequest.KeepAlive = true;

                /* Specify the Type of FTP Request */
                _ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;

                /* Establish Return Communication with the FTP Server */
                using var ftpResponse = (FtpWebResponse)await _ftpRequest.GetResponseAsync();

                _ftpRequest = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Rename File 
        /// </summary>
        /// <param name="currentFileNameAndPath"></param>
        /// <param name="newFileName"></param>
        public async void Rename(string currentFileNameAndPath, string newFileName)
        {
            try
            {
                /* Create an FTP Request */
                _ftpRequest = (FtpWebRequest)WebRequest.Create(_hostIP + "/" + currentFileNameAndPath);

                /* Log in to the FTP Server with the User Name and Password Provided */
                _ftpRequest.Credentials = new NetworkCredential(_userName, _password);

                /* When in doubt, use these options */
                _ftpRequest.UseBinary = true;
                _ftpRequest.UsePassive = true;
                _ftpRequest.KeepAlive = true;

                /* Specify the Type of FTP Request */
                _ftpRequest.Method = WebRequestMethods.Ftp.Rename;

                /* Rename the File */
                _ftpRequest.RenameTo = newFileName;

                /* Establish Return Communication with the FTP Server */
                using var ftpResponse = (FtpWebResponse)await _ftpRequest.GetResponseAsync();

                _ftpRequest = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Create a New Directory on the FTP Server 
        /// </summary>
        /// <param name="newDirectory"></param>
        public async void CreateDirectory(string newDirectory)
        {
            try
            {
                /* Create an FTP Request */
                _ftpRequest = (FtpWebRequest)WebRequest.Create(_hostIP + "/" + newDirectory);

                /* Log in to the FTP Server with the User Name and Password Provided */
                _ftpRequest.Credentials = new NetworkCredential(_userName, _password);

                /* When in doubt, use these options */
                _ftpRequest.UseBinary = true;
                _ftpRequest.UsePassive = true;
                _ftpRequest.KeepAlive = true;

                /* Specify the Type of FTP Request */
                _ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;

                /* Establish Return Communication with the FTP Server */
                using var ftpResponse = (FtpWebResponse)await _ftpRequest.GetResponseAsync();

                _ftpRequest = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Get the Date/Time a File was Created
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<string> GetFileCreatedDateTime(string fileName)
        {
            try
            {
                /* Create an FTP Request */
                _ftpRequest = (FtpWebRequest)WebRequest.Create(_hostIP + "/" + fileName);

                /* Log in to the FTP Server with the User Name and Password Provided */
                _ftpRequest.Credentials = new NetworkCredential(_userName, _password);

                /* When in doubt, use these options */
                _ftpRequest.UseBinary = true;
                _ftpRequest.UsePassive = true;
                _ftpRequest.KeepAlive = true;

                /* Specify the Type of FTP Request */
                _ftpRequest.Method = WebRequestMethods.Ftp.GetDateTimestamp;

                /* Establish Return Communication with the FTP Server */
                using var ftpResponse = (FtpWebResponse)await _ftpRequest.GetResponseAsync();

                /* Establish Return Communication with the FTP Server */
                await using var ftpStream = ftpResponse.GetResponseStream();

                if (ftpStream != null)
                {
                    /* Get the FTP Server's Response Stream */
                    using var ftpReader = new StreamReader(ftpStream);

                    /* Store the Raw Response */
                    string fileInfo = null;

                    /* Read the Full Response Stream */
                    try
                    {
                        fileInfo = await ftpReader.ReadToEndAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }

                    _ftpRequest = null;

                    /* Return File Created Date Time */
                    return fileInfo;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            /* Return an Empty string Array if an Exception Occurs */
            return "";
        }

        /// <summary>
        /// Get the Size of a File
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<string> GetFileSize(string fileName)
        {
            try
            {
                /* Create an FTP Request */
                _ftpRequest = (FtpWebRequest)WebRequest.Create(_hostIP + "/" + fileName);

                /* Log in to the FTP Server with the User Name and Password Provided */
                _ftpRequest.Credentials = new NetworkCredential(_userName, _password);

                /* When in doubt, use these options */
                _ftpRequest.UseBinary = true;
                _ftpRequest.UsePassive = true;
                _ftpRequest.KeepAlive = true;

                /* Specify the Type of FTP Request */
                _ftpRequest.Method = WebRequestMethods.Ftp.GetFileSize;

                /* Establish Return Communication with the FTP Server */
                using var ftpResponse = (FtpWebResponse)await _ftpRequest.GetResponseAsync();

                /* Establish Return Communication with the FTP Server */
                await using var ftpStream = ftpResponse.GetResponseStream();

                if (ftpStream != null)
                {
                    /* Get the FTP Server's Response Stream */
                    using var ftpReader = new StreamReader(ftpStream);

                    /* Store the Raw Response */
                    string fileInfo = null;

                    /* Read the Full Response Stream */
                    try
                    {
                        while (ftpReader.Peek() != -1)
                        {
                            fileInfo = await ftpReader.ReadToEndAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }

                    _ftpRequest = null;

                    /* Return File Size */
                    return fileInfo;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            /* Return an Empty string Array if an Exception Occurs */
            return "";
        }

        /// <summary>
        /// List Directory Contents File/Folder Name Only
        /// </summary>
        /// <returns></returns>
        public async Task<string[]> GetDirectoryListSimple(string directory)
        {
            try
            {
                /* Create an FTP Request */
                _ftpRequest = (FtpWebRequest)WebRequest.Create(_hostIP + "/" + directory);

                /* Log in to the FTP Server with the User Name and Password Provided */
                _ftpRequest.Credentials = new NetworkCredential(_userName, _password);

                /* When in doubt, use these options */
                _ftpRequest.UseBinary = true;
                _ftpRequest.UsePassive = true;
                _ftpRequest.KeepAlive = true;

                /* Specify the Type of FTP Request */
                _ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;

                /* Establish Return Communication with the FTP Server */
                using var ftpResponse = (FtpWebResponse)await _ftpRequest.GetResponseAsync();

                /* Establish Return Communication with the FTP Server */
                await using var ftpStream = ftpResponse.GetResponseStream();

                if (ftpStream != null)
                {
                    /* Get the FTP Server's Response Stream */
                    using var ftpReader = new StreamReader(ftpStream);

                    /* Store the Raw Response */
                    string directoryRaw = null;

                    /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
                    try
                    {
                        while (ftpReader.Peek() != -1)
                        {
                            directoryRaw += await ftpReader.ReadLineAsync() + "|";
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }

                    _ftpRequest = null;

                    /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
                    try
                    {
                        if (directoryRaw is not null)
                        {
                            var directoryList = directoryRaw.Split("|".ToCharArray());
                            return directoryList;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            /* Return an Empty string Array if an Exception Occurs */
            return new[] { "" };
        }

        /// <summary>
        /// List Directory Contents in Detail (Name, Size, Created, etc.)
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public async Task<string[]> GetDirectoryListDetailed(string directory)
        {
            try
            {
                /* Create an FTP Request */
                _ftpRequest = (FtpWebRequest)WebRequest.Create(_hostIP + "/" + directory);

                /* Log in to the FTP Server with the User Name and Password Provided */
                _ftpRequest.Credentials = new NetworkCredential(_userName, _password);

                /* When in doubt, use these options */
                _ftpRequest.UseBinary = true;
                _ftpRequest.UsePassive = true;
                _ftpRequest.KeepAlive = true;

                /* Specify the Type of FTP Request */
                _ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                /* Establish Return Communication with the FTP Server */
                using var ftpResponse = (FtpWebResponse)await _ftpRequest.GetResponseAsync();

                /* Establish Return Communication with the FTP Server */
                await using var ftpStream = ftpResponse.GetResponseStream();

                if (ftpStream != null)
                {
                    /* Get the FTP Server's Response Stream */
                    using var ftpReader = new StreamReader(ftpStream);
                    /* Store the Raw Response */
                    string directoryRaw = null;
                    /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
                    try
                    {
                        while (ftpReader.Peek() != -1)
                        {
                            directoryRaw += await ftpReader.ReadLineAsync() + "|";
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }

                    _ftpRequest = null;

                    /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
                    try
                    {
                        if (directoryRaw is not null)
                        {
                            var directoryList = directoryRaw.Split("|".ToCharArray());
                            return directoryList;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            /* Return an Empty string Array if an Exception Occurs */
            return new[] { "" };
        }
    }
}