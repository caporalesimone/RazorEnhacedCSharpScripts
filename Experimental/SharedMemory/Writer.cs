//C#
using Assistant;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace RazorEnhanced
{
    public class SharedMemoryWriter
    {
        private struct CharacterData
        {
            public Point3D position;
        }


        public class SharedMemoryMapper<T> where T : struct
        {
            private string smName;
            private Mutex smLock;
            private int smSize;
            private bool locked;
            private MemoryMappedFile mmf;
            private MemoryMappedViewAccessor accessor;

            public SharedMemoryMapper(string name, int size)
            {
                smName = name;
                smSize = size;
            }

            public bool Open()
            {
                try
                {
                    // Create named MMF
                    mmf = MemoryMappedFile.CreateOrOpen(smName, smSize);

                    // Create accessors to MMF
                    accessor = mmf.CreateViewAccessor(0, smSize, MemoryMappedFileAccess.ReadWrite);

                    // Create lock
                    smLock = new Mutex(true, "SM_LOCK", out locked);
                }
                catch
                {
                    return false;
                }

                return true;
            }

            public void Close()
            {
                accessor.Dispose();
                mmf.Dispose();
                smLock.Close();
            }

            public T DataBlock
            {
                get
                {
                    T dataStruct;
                    accessor.Read<T>(0, out dataStruct);
                    return dataStruct;
                }
                set
                {
                    smLock.WaitOne();
                    accessor.Write<T>(0, ref value);
                    smLock.ReleaseMutex();
                }
            }
        }



        Mobile mobilePlayer = Mobiles.FindBySerial(Player.Serial);

        private void Log(object messageString)
        {
            Misc.SendMessage(messageString, 201);
        }

        public void Run()
        {
            CharacterData characterData = new CharacterData();
            
            SharedMemoryMapper<CharacterData> sharedMemoryMapper = new SharedMemoryMapper<CharacterData>(Player.Serial.ToString(), 128);
            if (!sharedMemoryMapper.Open()) return;

            while (true)
            {
                characterData.position = Player.Position;
                sharedMemoryMapper.DataBlock = characterData;
                Application.DoEvents();
                Misc.Pause(10);
            }

            // sharedMemoryMapper.Close();

        }
    }



}