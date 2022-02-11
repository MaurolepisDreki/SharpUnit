using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Unit {
	public struct LibInfo {
		public     const            string    NAME      =  "SharpUnit";
		public     static readonly  string[]  AUTHOR    =  { "Maurolepis Dreki" };
		public     static readonly  string[]  EMAIL     =  { "alpha at the fenrir unchained dot info" };
		public struct VERSION {
			public  const            uint      MAJOR     =  2022;
			public  const            uint      MINOR     =  29;
			public  const            uint      REVISION  =  4862395;
			public  static readonly  string    STRING    =  $"V{MAJOR}.{MINOR:D3} R{REVISION:D7}";
		}
	}

	// Base Exception Type for SharpUnit
	public class UnitException : Exception {
		public UnitException( string message ) : base( message ) {
			// Do Nothing
		}
	}

	// Base Obejct Type of SharpUnit
	// vvv -- Should be private or internal, but C# doesn't allow that...
	public abstract class Component {
		protected static StackFrame GetFrame( Type obj ) {
			// TODO: How do you get the child type?
			StackFrame[] stack = new StackTrace().GetFrames();
			foreach( StackFrame frame in stack ) {
				if( frame.GetMethod()!.ReflectedType == obj ) {
					return frame;
				}
			}

			// else
			throw new UnitException( $"{obj.ToString()} is not in stack." );
		}
	}
	
	// Base Exception Type thrown by the Engine Component
	public class EngineException : UnitException {
		public EngineException( string message ) : base( message ) {
			// Do Nothing
		}
	}

	// SharpUnit's Engine Component
	public class Engine : Component {
		private Action? _setup;
		private Action? _clean;
		private List<Suite> _suitelist;

		public Engine( Action? setup = null, Action? clean = null ) {
				_setup = setup;
				_clean = clean;
				_suitelist = new List<Suite>();
		}

		public void SetSetup( Action? setup ) {
			_setup = setup;
		}

		public void SetCleanup( Action? clean ) {
			_clean = clean;
		}

		public Suite NewSuite( string name, Action? setup = null, Action? clean = null ) {
			Suite mySuite = new Suite( name, setup, clean );
			AddSuite( mySuite );
			return mySuite;
		}

		public void AddSuite( Suite suite ) {
			if( _suitelist.IndexOf( suite ) < 0 ) {
				_suitelist.Add( suite );
			} else {
					throw new EngineException( "Duplicate Suite" );
			}
		}

		public void Run() {
			AssertFatal? ex = null;
			if( _setup != null ) _setup();
			try{ foreach( Suite suite in _suitelist ) suite.Run(); }
			catch( AssertFatal e ){ ex = e; }
			if( _clean != null ) _clean();
			if( ex != null ) Console.WriteLine( "       FATAL" );
		}
	}


	// Base Exception thrown by Suites
	public class SuiteException : UnitException {
		public SuiteException( string message ) : base( message ) {
			// Do Nothing
		}
	}

	// SharpUnit's Suite Object
	public class Suite : Component {
		private string _name;
		private Action? _setup;
		private Action? _clean;
		private List<Test> _testlist;
		private bool _enabled;

		public string Name {
			get{ return _name; }
		}

		public bool Enabled {
			get{ return _enabled; }
		}

		public void Enable() {
			_enabled = true;
		}

		public void Disable() {
			_enabled = false;
		}

		public Suite( string name, Action? setup = null, Action? clean = null ) {
			_name = name;
			_setup = setup;
			_clean = clean;
			_testlist = new List<Test>();
			_enabled = true;
		}

		public Test NewTest( string name, Action test, Action? setup = null, Action? clean = null ) {
			Test myTest = new Test( name, test, setup, clean );
			AddTest( myTest );
			return myTest;
		}

		public void AddTest( Test test ) {
			if( _testlist.IndexOf( test ) < 0 ) {
				_testlist.Add( test );
			} else {
				throw new SuiteException( "Duplicate Test" );
			}
		}

		public void Run() {
			if( _enabled ) {
				AssertFatal? ex = null;
				Console.WriteLine( " SUITE: {0}:", _name );
				if( _setup != null ) _setup();
				try{ foreach( Test test in _testlist ) test.Run(); }
				catch( AssertFatal e ) { ex = e; }
				if( _clean != null ) _clean();
				if( ex != null ) throw ex; //< Pass our captured exception up the stack
			}
		}
	}


	// Base Exception thrown by Tests
	public class TestException : UnitException {
		public TestException( string message ) : base( message ) {
			// Do Nothing
		}
	}

	// SharpUnit's Test Object
	public class Test : Component {
		#region Master Test Registry
		private static Dictionary<MethodInfo, Test> _testregister = new Dictionary<MethodInfo, Test>();

		private static void Register( MethodInfo fingerprint, Test instance ) {
			if( _testregister.ContainsKey( fingerprint ) ) throw new TestException( "Duplicate Test" );
			_testregister.Add( fingerprint, instance );
		}

		private static void Unregister( Test instance ) {
			_testregister.Remove( instance._test.Method );
		}

		public static Test Lookup( MethodInfo fingerprint ) {
			if( ! _testregister.ContainsKey( fingerprint ) ) throw new TestException( "Test not registered" );
			return _testregister[ fingerprint ];
		}
		#endregion

		#region Test Implimentation
		private string _name;
		private Action? _setup;
		private Action _test;
		private Action? _clean;
		private List<Assert> _assertlist;
		private bool _enabled;

		public string Name {
			get{ return _name; }
		}

		public bool Passed {
			get {
				foreach( Assert assert in _assertlist )
					if( ! assert.Passed )
						return false;
				//else
				return true;
			}
		}

		public bool Enabled {
			get{ return _enabled; }
		}

		public void Enable() {
			_enabled = true;
		}

		public void Disable() {
			_enabled = false;
		}

		public Test( string name, Action test, Action? setup = null, Action? clean = null ) {
			if( test == null ) throw new TestException( "Empty Test" );

			_name = name;
			_setup = setup;
			_test = test;
			_clean = clean;
			_assertlist = new List<Assert>();
			_enabled = true;

			Test.Register( _test.Method, this );
		}

		~Test() {
			Test.Unregister( this );
		}

		public void AddAssert( Assert assert ) {
			_assertlist.Add( assert );
		}

		public void Run() {
			if( _enabled ) {
				AssertFatal? ex = null;
				Console.Write( "   TEST: {0}... ", _name );
				_assertlist.Clear(); //< Fresh Run
				if( _setup != null ) _setup();
				try{ _test(); }
				catch( AssertFatal e ) { ex = e; }
				if( _clean != null ) _clean();
				Console.WriteLine( Passed ? "OK" : "FAIL" );

				/* Log Errors */
				for( int i = 0; i < _assertlist.Count; i++ ) {
					if( ! _assertlist[ i ].Passed ) {
						Console.Write( "     [{0}] ", i );
						_assertlist[ i ].Print();
					}
				}

				/* Pass Exception up the stack */
				if( ex != null ) throw ex;
			}
		}
		#endregion
	}


	// Base Exception thrown by Assertations
	public class AssertException : UnitException {
		public AssertException( string message ) : base( message ) {
			// Do Nothing
		}
	}

	// Special Exception thrown by Assertations to signal the Engine to exit gracefully
	public class AssertFatal : AssertException {
		public AssertFatal( string message ) : base( message ) {
			// Do Less Than Nothing
		}
	}

	// Assertation Object/Organizer
	//   used to contain the different assertations which log their results to their calling tests
	public class Assert : Component {
		private string _what;
		private string _file;
		private int _line;
		private bool _passed;

		public bool Passed {
			get{ return _passed; }
		}

		private Assert( string what, string file, int line, bool passed ) {
			_what = what;
			_file = file;
			_line = line;
			_passed = passed;
		}

		public void Print() {
			Console.WriteLine( "{0} @{1}: {2}", _file, _line, _what );
		}

		private static Test GetTest() {
			StackFrame[] stack = new StackTrace().GetFrames();
			for( int i = 0; i < stack.Length; i++ ) {
				try {
					return Test.Lookup( (MethodInfo)stack[i].GetMethod()! );
				} catch( TestException ) { /* Keep Looking */ }
			}
			throw new AssertException( "Not in test" );
		}

		private static void AddAssert( Assert assert ) {
			GetTest().AddAssert( assert );
		}

		public static void Pass( [CallerFilePath] string file = "", [CallerLineNumber] int line = 0 ) {
			AddAssert( new Assert( "PASS", file, line, true ) );
		}

		public static void Fail( string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0 ) {
			AddAssert( new Assert( message, file, line, false ) );
		}

		public static void Fatal( string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0 ) {
			Fail( message, file, line );
			throw new AssertFatal( message );
		}

		public static void True( bool expr, [CallerArgumentExpression( "expr" )] string message = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0 ) {
			if( expr ) Pass( file, line );
			else Fail( message, file, line );
		}

		public static void True_Fatal( bool expr, [CallerArgumentExpression( "expr" )] string message = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0 ) {
			if( expr ) Pass( file, line );
			else Fatal( message, file, line );
		}

		public static void False( bool expr, [CallerArgumentExpression( "expr" )] string message = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0 ) {
			if( expr ) Fail( message, file, line );
			else Pass( file, line );
		}

		public static void False_Fatal( bool expr, [CallerArgumentExpression( "expr" )] string message = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0 ) {
			if( expr ) Fatal( message, file, line );
			else Pass( file, line );
		}		
	}
}
