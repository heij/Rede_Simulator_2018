using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class MyClass
    {
        public abstract class BaseLayer<T>
        {

            public class MessageReceivedEvent<T> : EventArgs
            {
                public T data { get; set; }
            }

            public void MessageReceived(object sender, EventArgs args) {

            }

            public void ProcessAndSendToNext(T data, BaseLayer<T> nLayer, string method)
            {

            }

            public abstract T Encode(string e);

            public abstract string Decode(T e);

        }

        class Layer1 : BaseLayer<object>
        {
            public override object Encode(string e)
            {
                Console.WriteLine("Entrada: " + e);
                return e;
            }

            public override string Decode(object e)
            {
                //Console.WriteLine("Saída: " + e);
                return e.ToString();
            }
        }

        class Layer2 : BaseLayer<object>
        {
            public override object Encode(string e)
            {
                char[] strArray = e.ToCharArray();
                Array.Reverse(strArray);
                
                return new string(strArray);
            }

            public override string Decode(object e)
            {
                var e2 = String.Join("", e);
                return e2;
            }
        }

        class Layer3 : BaseLayer<object>
        {
            public override object Encode(string e)
            {
                char[] strArray = e.ToCharArray();

                return strArray;
            }

            public override string Decode(object e)
            {
                char[] eArr = (e as List<char>).ToArray();

                Array.Reverse(eArr);
                return new string(eArr);
            }
        }

        class Node
        {
            // ID
            private string name { get; set; }
            public Node(string name)
            {
                this.name = name;
            }

            private IDictionary<string, List<char>> Comms = new Dictionary<string, List<char>>();

            public event EventHandler MessageObserver;

            public class MessageEventArgs<T> : EventArgs
            {
                public T Msg { get; set; }
                public Node Target { get; set; }

                public MessageEventArgs(T Msg, Node Target)
                {
                    this.Msg = Msg;
                    this.Target = Target;
                }
            }

            public void MessageReceived(object sender, EventArgs args)
            {
                string senderId = ((Node)sender).name;
                
                MessageEventArgs<char> senderMessage = args as MessageEventArgs<char>;
                //((Node)senderMessage.Target).name

                // If the sent value is of type char;
                if (senderMessage != null)
                {
                    if (senderMessage.Target.name == name)
                    {
                        Console.WriteLine("{0}: {1} sent a message; Content: {2}", name, senderId, senderMessage.Msg);
                        Comms[senderId].Add(senderMessage.Msg);
                        return;
                    }
                } else
                {
                    // If the sent value is of type bool (requesting start or end of communication);
                    MessageEventArgs<bool> senderToggle = args as MessageEventArgs<bool>;

                    if (senderToggle.Target.name == name)
                    {
                        switch (senderToggle.Msg)
                        {
                            case true:
                                Console.WriteLine("{0}: {1} wants to send a message;", name, senderId);
                                Comms[senderId] = new List<char>();
                                break;

                            default:
                                Console.WriteLine("{0}: {1} finished sending the message;", name, senderId);
                                Console.WriteLine("{0}: Decoded {1}'s message: {2}", name, senderId, Decode(Comms[senderId]));
                                break;
                        }
                        return;
                    }
                }
                Console.WriteLine("{0}: {1} broadcasted a message for another node; Ignoring;", name, senderId);

            }

            // Layers;
            private BaseLayer<object>[] Layers = new BaseLayer<object>[] {
                new Layer1(),
                new Layer2(),
                new Layer3()
            };

            public char[] Encode(string e)
            {
                Object[] encodedReturns = new Object[Layers.Length + 1];
                encodedReturns[0] = e;

                for (int i = 0; i < Layers.Length; i++)
                {
                    encodedReturns[i + 1] = Layers[i].Encode((string)encodedReturns[i]);
                }

                return encodedReturns[Layers.Length] as char[];
            }

            public string Decode(List<char> e)
            {
                Object[] decodedReturns = new Object[Layers.Length + 1];
                decodedReturns[0] = e;

                for (int i = Layers.Length - 1, c = 0; i >= 0 ; i--, c++)
                {
                    decodedReturns[c + 1] = Layers[i].Decode(decodedReturns[c]);
                }
                
                return decodedReturns[Layers.Length] as string;
            }

            public void SendMessage(string e, Node m)
            {
                char[] Encoded = Encode(e);

                // Open communication channel;
                SendChar(true, m);

                foreach (char c in Encoded)
                {
                    // Send each char;
                    SendChar(c, m);
                }

                // Close communication channel;
                SendChar(false, m);
            }

            public void SendChar<T>(T c, Node m)
            {
                EventHandler handler = MessageObserver;

                MessageEventArgs<T> msg = new MessageEventArgs<T>(c, m);
                handler?.Invoke(this, msg);
            }

        }

        static void Main(string[] args) {
            Node NodeA = new Node("Node A");
            Node NodeB = new Node("Node B");
            Node NodeC = new Node("Node C");

            NodeA.MessageObserver += NodeB.MessageReceived;
            NodeA.MessageObserver += NodeC.MessageReceived;

            NodeA.SendMessage("To aqui", NodeB);

            Console.ReadKey();
        }

    }
}