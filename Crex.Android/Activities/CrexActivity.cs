using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Views;

namespace Crex.Android.Activities
{
    [Activity( Label = "Crex")]
	public class CrexActivity : Activity
    {
        public static CrexActivity MainActivity { get; private set; }

        List<CrexBaseFragment> Fragments = new List<CrexBaseFragment>();

        public override void OnCreate( Bundle savedInstanceState, PersistableBundle persistentState )
        {
            base.OnCreate( savedInstanceState, persistentState );

            MainActivity = this;
        }

        protected override void OnStart()
        {
            MainActivity = this;
            base.OnStart();

            Console.WriteLine( "Start" );

            Crex.Application.Current.StartAction( this, Crex.Application.Current.Config.ApplicationRootUrl );
        }

        public void PushFragment(CrexBaseFragment fragment)
        {
            var tx = FragmentManager.BeginTransaction();
            tx.SetTransition( FragmentTransit.FragmentOpen );
            Fragments.Add( fragment );
            tx.Add( 16908290, fragment ); /* Content */
            tx.Commit();
        }

        public void PopLastFragment()
        {
            if ( Fragments.Count > 0 )
            {
                var tx = FragmentManager.BeginTransaction();
                tx.SetTransition( FragmentTransit.FragmentClose );
                tx.Remove( Fragments.Last() );
                Fragments.Remove( Fragments.Last() );
                tx.Commit();
            }
        }

        public override bool DispatchKeyEvent( KeyEvent e )
        {
            Console.WriteLine( e.KeyCode.ToString() );

            if ( e.KeyCode == Keycode.DpadUp )
            {
                var tx = FragmentManager.BeginTransaction();
                tx.SetTransition( FragmentTransit.FragmentOpen );
                var frag = new Activities.MenuFragment();
                //Fragments[0].View.Visibility = ViewStates.Invisible;
                Fragments.Add( frag );
                tx.Add( 16908290, frag ); /* Content */
                tx.Commit();

                return true;
            }

            if ( e.KeyCode == Keycode.DpadDown && Fragments.Count > 1 )
            {
                PopLastFragment();

                return true;
            }

            return base.DispatchKeyEvent( e );
        }
    }
}
