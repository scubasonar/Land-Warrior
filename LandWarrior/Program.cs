using System;
using System.Threading;
using System.Text;
using System.IO.Ports;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;

namespace LandWarrior
{
    public class Program
    {
        // Motor related
        private static PWM leftMotor = new PWM((PWM.Pin)FEZ_Pin.PWM.Di5);
        private static PWM rightMotor = new PWM((PWM.Pin)FEZ_Pin.PWM.Di10);
        private static OutputPort DIRA = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di12, true);
        private static OutputPort DIRB = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di13, false);    
        private static int leftPwr = 0; // what power level 0-100 to set the left motor to
        private static int rightPwr = 0; // ... to set the right motor to

        // Radio stuff
        public static SerialPort Radio = null;
        public static int read_count = 0;
        public static byte[] rx_data = new byte[256];
        public static byte[] tx_data;
        public static byte[] str = new byte[256];

        public static void Main()
        {
            Radio = new SerialPort("COM1", 9600);
            Radio.Open();

         //   Radio.DataReceived += new SerialDataReceivedEventHandler(UART_DataReceived);

            uint i = 0;
            while (true)
            {
                if (leftPwr != 0)
                    leftMotor.SetPulse(100000, (uint)(((double)leftPwr / 100) * 90000.0));
                else
                    leftMotor.SetPulse(100000, 0);

                if (rightPwr != 0)
                    rightMotor.SetPulse(100000, (uint)(((double)rightPwr / 100) * 90000.0));
                else
                    rightMotor.SetPulse(100000, 0);
                //leftMotor.SetPulse(100000, 90000);
                Thread.Sleep(1);
                string input = "";

                // read the data
                try
                {
                    read_count = Radio.Read(rx_data, 0, Radio.BytesToRead);

                    if (read_count > 0)
                    {
                        for (int k = 0; k < read_count; k++ )
                        {
                            if (rx_data[k] == '$')
                                i = 0;
                            else if (rx_data[k] == '*')
                            {
                                Radio.Flush();
                                for(int j = 0; j < i; j++)
                                {
                                    input += (char)str[j];
                                }
                                ParseCommand(input);
                                i = 0;
                                read_count = 0;
                                
                            }
                            else
                            {
                                str[i] = rx_data[k];
                                i++;
                                if (i > str.Length) i = 0;
                            }
                        }
                    }
                }
                catch
                {
                }

                
            }
        }

        public static void Init()
        {
            leftMotor.Set(false);
            rightMotor.Set(false);

            DIRB.Write(true); // right side go forward
            DIRA.Write(false); //left side go forward

        }

        private static void UART_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string input = "";

            // read the data
            try
            {
                read_count = Radio.Read(rx_data, 0, Radio.BytesToRead);
            }
            catch
            {
            }

            foreach (byte b in rx_data)
            {
                input += (char)b;
            }
            Debug.Print(input);
            ParseCommand(input);
            
            Radio.Flush();
            
        }

        private static void ParseCommand(string cmd)
        {
            
            string[] strings = cmd.Split(new char[] { ',' });
            
            // set the left motor
            try
            {
                leftPwr = int.Parse(strings[0]);
                if (leftPwr < 0)
                {
                    DIRA.Write(true);
                    leftPwr *= -1;
                }
                else
                {
                    DIRA.Write(false);
                }
            }
            catch
            {
                leftPwr = 0;
            }

            // set the right motor
            try
            {
                rightPwr = int.Parse(strings[1]);
                if (rightPwr < 0)
                {
                    DIRB.Write(false);
                    rightPwr *= -1;
                }
                else
                {
                    DIRB.Write(true);
                }
            }
            catch
            {
                rightPwr = 0;
            }
        }

        
    }

  
}
