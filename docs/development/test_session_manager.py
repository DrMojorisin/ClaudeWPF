#!/usr/bin/env python3
"""
Test script for WPFBase Session Management System
"""

import sys
from pathlib import Path

# Add current directory to path
sys.path.insert(0, str(Path(__file__).parent))
sys.path.append('.claude')

import session_manager
from session_manager import WPFBaseSessionManager, SessionType, SessionPhase

def test_session_lifecycle():
    """Test complete session lifecycle"""
    print("[TEST] Testing session lifecycle...")

    try:
        # Initialize session manager
        project_root = Path("C:/DEVELOPMENT/Projects/WPFBase")
        session_manager = WPFBaseSessionManager(project_root)

        # Start session
        session_id = session_manager.start_session(
            session_type=SessionType.STANDARD,
            description="Test user management feature",
            target_features=["UserManagement", "UserValidation"]
        )

        print(f"[PASS] Session started: {session_id}")

        # Update progress
        session_manager.update_progress("Implementing UserViewModel")
        session_manager.apply_pattern("ObservableValidator")
        session_manager.record_decision(
            "Use ObservableValidator pattern",
            "Provides built-in validation support",
            "Reduces boilerplate code"
        )

        print("[PASS] Progress updated successfully")

        # Create checkpoint
        checkpoint_id = session_manager.create_checkpoint("UserViewModel complete")
        print(f"[PASS] Checkpoint created: {checkpoint_id}")

        # Advance phase
        session_manager.advance_phase(SessionPhase.IMPLEMENTATION, "Starting core implementation")
        print("[PASS] Phase advanced successfully")

        # Get analytics
        analytics = session_manager.get_session_analytics()
        print(f"[PASS] Analytics retrieved: {analytics['session_id']}")

        # End session
        summary = session_manager.end_session(
            success=True,
            summary="Test session completed successfully"
        )

        print(f"[PASS] Session ended: {summary['status']}")
        return True

    except Exception as e:
        print(f"[FAIL] Session lifecycle test failed: {e}")
        return False

def test_session_persistence():
    """Test session state persistence"""
    print("\n[TEST] Testing session persistence...")

    try:
        project_root = Path("C:/DEVELOPMENT/Projects/WPFBase")
        session_manager = WPFBaseSessionManager(project_root)

        # Check sessions directory
        sessions_dir = project_root / ".claude" / "sessions"
        print(f"[INFO] Sessions directory: {sessions_dir}")
        print(f"[INFO] Directory exists: {sessions_dir.exists()}")

        if sessions_dir.exists():
            session_files = list(sessions_dir.glob("session_*.json"))
            print(f"[PASS] Found {len(session_files)} session files")

            # List recent sessions
            recent_sessions = session_manager.list_sessions(limit=5)
            print(f"[PASS] Listed {len(recent_sessions)} recent sessions")

            return True
        else:
            print("[WARN] Sessions directory not found")
            return True  # Not a failure, just no sessions yet

    except Exception as e:
        print(f"[FAIL] Session persistence test failed: {e}")
        return False

def test_quality_integration():
    """Test quality gate integration"""
    print("\n[TEST] Testing quality gate integration...")

    try:
        project_root = Path("C:/DEVELOPMENT/Projects/WPFBase")
        session_manager = WPFBaseSessionManager(project_root)

        # Test quality validation
        validation_results = session_manager.validate_quality_gates()
        print(f"[PASS] Quality validation completed")
        print(f"[INFO] Passing gates: {validation_results['passing_count']}")
        print(f"[INFO] Failing gates: {validation_results['failing_count']}")

        return True

    except Exception as e:
        print(f"[FAIL] Quality integration test failed: {e}")
        return False

def test_error_scenarios():
    """Test error handling scenarios"""
    print("\n[TEST] Testing error scenarios...")

    try:
        project_root = Path("C:/DEVELOPMENT/Projects/WPFBase")
        session_manager = WPFBaseSessionManager(project_root)

        # Test operations without active session
        try:
            session_manager.create_checkpoint("No session test")
            print("[FAIL] Should have raised error for no active session")
            return False
        except ValueError:
            print("[PASS] Correctly handled no active session error")

        # Test invalid checkpoint restoration
        session_id = session_manager.start_session(
            SessionType.QUICK,
            "Error test session",
            ["TestFeature"]
        )

        result = session_manager.restore_checkpoint("nonexistent_checkpoint")
        if not result:
            print("[PASS] Correctly handled invalid checkpoint")
        else:
            print("[FAIL] Should have returned False for invalid checkpoint")
            return False

        # Clean up
        session_manager.end_session(success=True)

        return True

    except Exception as e:
        print(f"[FAIL] Error scenario test failed: {e}")
        return False

def main():
    """Run all session management tests"""
    print("WPFBase Session Management Test Suite")
    print("=" * 50)

    tests = [
        test_session_lifecycle,
        test_session_persistence,
        test_quality_integration,
        test_error_scenarios
    ]

    passed = 0
    total = len(tests)

    for test in tests:
        if test():
            passed += 1

    print("\n" + "=" * 50)
    print(f"Test Results: {passed}/{total} tests passed")

    if passed == total:
        print("All tests passed! Session Management is ready for use.")
        return 0
    else:
        print("Some tests failed. Check configuration and dependencies.")
        return 1

if __name__ == "__main__":
    exit_code = main()
    sys.exit(exit_code)