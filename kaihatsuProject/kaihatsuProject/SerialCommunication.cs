using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.ComponentModel;
using System.IO.Ports;

namespace kaihatsuProject
{
    class SerialCommunication : IMySetter, INotifyPropertyChanged
    {//まだ途中? 作成→MessageReceivedに受信時イベントセット→あとはデータ受信したら勝手に読んでイベント着火してくれる
        SerialPort serialPort;
        string messageStr;
        string strToSend;
        public event MyEventHandler MessageReceived;
        public event MyEventHandler ErrorReceived;

        public string MessageLabel
        {//バインディング用, 読み取ったメッセージを示す
            get { return messageStr; }
            private set
            {
                messageStr = value;
                RaisePropertyChanged("MessageLabel");//UIに更新通知
            }
        }

        public SerialCommunication(string pName, int bRate)
        {
            serialPort = new SerialPort();
            serialPort.PortName = pName;
            serialPort.BaudRate = bRate;
            serialPort.DataBits = 8;
            serialPort.Parity = Parity.None;//パリティ無し
            serialPort.StopBits = StopBits.One;//ストップビット長
            serialPort.Handshake = Handshake.None;//RTSフローとXONN/XOFF制御を使用しない
            serialPort.DiscardNull = true;//nullバイト無視

            serialPort.ErrorReceived += OnErrorReceived;
            serialPort.DataReceived += OnDataReceived;

            strToSend = "";//暫定
        }

        private void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Console.WriteLine("SerialCommunication portName：" + serialPort.PortName + " 何らかのエラーが検知されました");
            MessageBox.Show("シリアル通信エラー検知");
            if (ErrorReceived != null)
            {
                MyEventArgs e1 = new MyEventArgs();
                ErrorReceived(this, e1);
            }
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {//これでいけるならNamedPipeCommunicationみたいな面倒なことしなくていいのでは
            int n = 0;
            byte[] buffer = new byte[100];
            try { n = serialPort.Read(buffer, 0, 100); }//ここで待ちが発生しないはず

            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            Array.Resize(ref buffer, n);
            MessageLabel = System.Text.Encoding.ASCII.GetString(buffer);
            if (MessageReceived != null)
            {
                MyEventArgs e2 = new MyEventArgs();
                e2.message = messageStr;
                MessageReceived(this, e2);
            }
        }

        public void Open()
        {
            if (!serialPort.IsOpen)
            {
                try { serialPort.Open(); }
                catch (Exception e)
                {
                    Console.WriteLine("SerialCommunication portName：" + serialPort.PortName + " シリアルポートのオープンに失敗しました");
                    MessageBox.Show(e.ToString());
                    return;
                }
                Console.WriteLine("SerialCommunication portName：" + serialPort.PortName + " シリアルポートのオープンに成功しました");
            }            
        }

        public void Close()
        {
            if (serialPort.IsOpen) { serialPort.Close(); }
        }

        
        public void Write(string str)
        {
            if (serialPort.IsOpen)
            {
                serialPort.Write(str);
            }
        }

        public void Write()//暫定
        {
            if (serialPort.IsOpen) serialPort.Write(strToSend);
        }

        public void LoadMember(List<string> memNames, List<string> memValues)
        {//メモ帳からロードするLoadSettingsの関数にこのクラスをぶち込むために必要なインターフェース(暫定版)
            for (int i = 0; i < memNames.Count; i++)
            {
                if ("portname" == memNames[i].ToLower())
                {
                    serialPort.PortName = memValues[i];
                    Console.WriteLine("SerialCommunication PortName：" + serialPort.PortName + " が読み込まれました");
                }
                else if ("baudrate" == memNames[i].ToLower())
                {
                   serialPort.BaudRate = int.Parse(memValues[i]);
                    Console.WriteLine("SerialCommunication BaudRate：" + serialPort.BaudRate.ToString() + " が読み込まれました");
                }
                else if ("strtosend" == memNames[i].ToLower())
                {
                    strToSend = memValues[i];
                    Console.WriteLine("SerialCommunication StrToSend：" + strToSend + " が読み込まれました");
                }
            }
        }
        public void SaveMember(List<string> memNames, List<string> memValues)
        {
            //未実装
        }

        //以下がviewmodelのクラスには必要らしい
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            var d = PropertyChanged;
            if (d != null)
                d(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
