using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PrettyPrintClipboardPls
{
    public class CopyPasteInterceptor : IDisposable
    {
        protected Dictionary<Keys, bool> m_controlKeysState;
        private KeysInterceptor m_keysInterceptor;

        public delegate void CopyPasteReceivedHandler();
        public event CopyPasteReceivedHandler CopyPasteReceived;

        public CopyPasteInterceptor()
        {
            this.m_keysInterceptor = new KeysInterceptor();

            KeysInterceptor.KeyboardKeyDown += InterceptKeys_KeyboardDown;
            KeysInterceptor.KeyboardKeyUp += InterceptKeys_KeyboardKeyUp;

            m_controlKeysState = new Dictionary<Keys, bool>();
            m_controlKeysState.Add(Keys.LControlKey, false);
            m_controlKeysState.Add(Keys.RControlKey, false);
        }

        private void InterceptKeys_KeyboardKeyUp(Keys key)
        {
            if (m_controlKeysState.ContainsKey(key))
            {
                m_controlKeysState[key] = false;
            }

            if (key == Keys.C && 
                (m_controlKeysState[Keys.LControlKey] == true || m_controlKeysState[Keys.RControlKey]))
            {
                CopyPasteReceived?.Invoke();
            }
        }

        private void InterceptKeys_KeyboardDown(Keys key)
        {
            if (m_controlKeysState.ContainsKey(key))
            {
                m_controlKeysState[key] = true;
            }
        }

        public void Dispose()
        {
            this.m_keysInterceptor.Dispose();
        }
    }
}