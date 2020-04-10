using System;
using System.ComponentModel;

namespace Sockets.EventArgs {
    public static class Extensions {
        /// <summary>
        /// Tries to safely invoke on the target thread the method represented by the current delegate.
        /// </summary>
        /// <param name="evt">The <see cref="Delegate"/> to invoke.</param>
        /// <param name="e">The <see cref="EventArgs"/> to be passed on to the event handler.</param>
        public static void SafeRaise(this Delegate evt, System.EventArgs e) {
            if (evt == null) return;

            foreach (Delegate singleCast in evt.GetInvocationList()) {
                var syncInvoke = singleCast.Target as ISynchronizeInvoke;

                if (syncInvoke != null && syncInvoke.InvokeRequired)
                    syncInvoke.BeginInvoke(singleCast, new object[] { e });
                else
                    singleCast.DynamicInvoke(e);
            }
        }
    }
}
