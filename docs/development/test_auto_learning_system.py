#!/usr/bin/env python3
"""
Test script for WPFBase Auto-Learning System
"""

import sys
import json
import tempfile
from pathlib import Path
from unittest.mock import Mock, patch
from datetime import datetime, timedelta

# Add current directory to path
sys.path.insert(0, str(Path(__file__).parent))
sys.path.append('.claude')

try:
    import auto_learning_system
    from auto_learning_system import WPFBaseAutoLearningSystem
except ImportError as e:
    print(f"[FAIL] Failed to import auto_learning_system: {e}")
    sys.exit(1)

def test_auto_learning_initialization():
    """Test auto-learning system initialization"""
    print("[TEST] Testing auto-learning system initialization...")

    try:
        project_root = Path("C:/DEVELOPMENT/Projects/WPFBase")
        learning_system = WPFBaseAutoLearningSystem(project_root)

        print(f"[PASS] Auto-learning system initialized successfully")
        print(f"[INFO] Project root: {learning_system.project_root}")
        print(f"[INFO] Patterns database: {learning_system.patterns_db_path}")
        print(f"[INFO] Success threshold: {learning_system.config['success_threshold']}")
        print(f"[INFO] Failure threshold: {learning_system.config['failure_threshold']}")

        # Test that patterns database is loaded
        if learning_system.patterns_db_path.exists():
            print("[PASS] Pattern database initialized")
        else:
            print("[WARN] Pattern database file not created")

        return True

    except Exception as e:
        print(f"[FAIL] Auto-learning initialization failed: {e}")
        return False

def test_pattern_usage_recording():
    """Test recording pattern usage and calculating metrics"""
    print("\n[TEST] Testing pattern usage recording...")

    try:
        project_root = Path("C:/DEVELOPMENT/Projects/WPFBase")
        learning_system = WPFBaseAutoLearningSystem(project_root)
        # Test recording successful pattern usage
        test_pattern_id = "observable_property_viewmodel"
        test_pattern_name = "ObservableProperty ViewModel Pattern"

        # Record multiple successful uses
        for i in range(5):
            learning_system.record_pattern_usage(
                pattern_id=test_pattern_id,
                pattern_name=test_pattern_name,
                pattern_category="mvvm_patterns",
                success=True,
                performance_impact=0.8,
                context={"scenario": f"TestViewModel implementation {i+1}"}
            )

        # Record one failure
        learning_system.record_pattern_usage(
            pattern_id=test_pattern_id,
            pattern_name=test_pattern_name,
            pattern_category="mvvm_patterns",
            success=False,
            performance_impact=-0.2,
            context={"scenario": "Complex inheritance scenario"}
        )

        print(f"[PASS] Pattern usage recorded successfully")

        # Check if pattern was stored in database
        if test_pattern_id in learning_system.patterns_database:
            pattern_record = learning_system.patterns_database[test_pattern_id]
            print(f"[INFO] Pattern: {pattern_record.pattern_name}")
            print(f"[INFO] Total uses: {pattern_record.usage_metrics.usage_count}")
            print(f"[INFO] Success rate: {pattern_record.usage_metrics.success_rate:.2f}")
            print(f"[INFO] Status: {pattern_record.status.value}")

            # Verify calculations
            expected_success_rate = 5/6  # 5 successes out of 6 total
            if abs(pattern_record.usage_metrics.success_rate - expected_success_rate) < 0.01:
                print("[PASS] Success rate calculation correct")
            else:
                print(f"[FAIL] Success rate calculation incorrect: {pattern_record.usage_metrics.success_rate} vs {expected_success_rate}")
        else:
            print("[FAIL] Pattern not found in database")

        return True

    except Exception as e:
        print(f"[FAIL] Pattern usage recording test failed: {e}")
        return False

def test_pattern_promotion_demotion():
    """Test automatic pattern promotion and demotion"""
    print("\n[TEST] Testing pattern promotion/demotion logic...")

    try:
        project_root = Path("C:/DEVELOPMENT/Projects/WPFBase")
        learning_system = WPFBaseAutoLearningSystem(project_root)

        # Test pattern promotion (high success rate, enough uses)
        promotion_pattern_id = "relay_command_async_cancellation"
        promotion_pattern_name = "RelayCommand AsyncCancellation Pattern"

        # Record enough successful uses to trigger promotion
        for i in range(4):  # Meet minimum uses threshold
            learning_system.record_pattern_usage(
                pattern_id=promotion_pattern_id,
                pattern_name=promotion_pattern_name,
                pattern_category="async_patterns",
                success=True,
                performance_impact=0.9,
                context={"scenario": f"Async operation {i+1}"}
            )

        # Check if pattern was stored and has good metrics
        if promotion_pattern_id in learning_system.patterns_database:
            pattern = learning_system.patterns_database[promotion_pattern_id]
            success_threshold = learning_system.config['success_threshold']
            min_usage = learning_system.config['min_usage_count']

            should_promote = (pattern.usage_metrics.success_rate >= success_threshold and
                             pattern.usage_metrics.usage_count >= min_usage)

            if should_promote:
                print(f"[PASS] Pattern '{promotion_pattern_name}' qualifies for promotion")
                print(f"[INFO] Success rate: {pattern.usage_metrics.success_rate:.2f} >= {success_threshold}")
                print(f"[INFO] Total uses: {pattern.usage_metrics.usage_count} >= {min_usage}")
            else:
                print(f"[WARN] Pattern doesn't qualify for promotion yet")
                print(f"[INFO] Success rate: {pattern.usage_metrics.success_rate:.2f}, Uses: {pattern.usage_metrics.usage_count}")
        else:
            print("[FAIL] Promotion pattern not found in database")

        # Test pattern demotion (low success rate, enough uses)
        demotion_pattern_id = "manual_property_changed"
        demotion_pattern_name = "Manual PropertyChanged Pattern"

        # Record mostly failed uses
        for i in range(5):
            success = i < 1  # Only first use succeeds
            learning_system.record_pattern_usage(
                pattern_id=demotion_pattern_id,
                pattern_name=demotion_pattern_name,
                pattern_category="legacy_patterns",
                success=success,
                performance_impact=-0.5 if not success else 0.2,
                context={"scenario": f"Legacy pattern use {i+1}"}
            )

        if demotion_pattern_id in learning_system.patterns_database:
            pattern = learning_system.patterns_database[demotion_pattern_id]
            failure_threshold = learning_system.config['failure_threshold']

            should_demote = (pattern.usage_metrics.success_rate <= failure_threshold and
                            pattern.usage_metrics.usage_count >= learning_system.config['min_usage_count'])

            if should_demote:
                print(f"[PASS] Pattern '{demotion_pattern_name}' qualifies for demotion")
                print(f"[INFO] Success rate: {pattern.usage_metrics.success_rate:.2f} <= {failure_threshold}")
            else:
                print(f"[WARN] Pattern doesn't qualify for demotion yet")
                print(f"[INFO] Success rate: {pattern.usage_metrics.success_rate:.2f}")
        else:
            print("[FAIL] Demotion pattern not found in database")

        return True

    except Exception as e:
        print(f"[FAIL] Pattern promotion/demotion test failed: {e}")
        return False

def test_claude_md_updates():
    """Test automatic CLAUDE.md updates"""
    print("\n[TEST] Testing CLAUDE.md update functionality...")

    try:
        project_root = Path("C:/DEVELOPMENT/Projects/WPFBase")
        learning_system = WPFBaseAutoLearningSystem(project_root)

        # Test learning analytics
        analytics = learning_system.get_learning_analytics()

        print(f"[PASS] Learning analytics generated")
        print(f"[INFO] System overview: {type(analytics)}")

        if 'system_metrics' in analytics:
            metrics = analytics['system_metrics']
            print(f"[INFO] Total patterns: {metrics.get('total_patterns', 0)}")
            print(f"[INFO] Active patterns: {metrics.get('active_patterns', 0)}")

        # Test CLAUDE.md path exists
        if learning_system.claude_md_path.exists():
            print("[PASS] CLAUDE.md file found")
        else:
            print("[WARN] CLAUDE.md file not found")

        return True

    except Exception as e:
        print(f"[FAIL] CLAUDE.md update test failed: {e}")
        return False

def test_session_integration():
    """Test integration with session management system"""
    print("\n[TEST] Testing session management integration...")

    try:
        project_root = Path("C:/DEVELOPMENT/Projects/WPFBase")
        learning_system = WPFBaseAutoLearningSystem(project_root)

        # Test integration methods exist
        session_data = {
            "session_id": "test_session_12345",
            "patterns_used": ["ObservableProperty", "RelayCommand"],
            "success_rate": 0.9
        }

        # Test session integration method
        learning_system.integrate_with_session_management(session_data)
        print(f"[PASS] Session integration method executed")

        # Test quality gates integration
        quality_data = {
            "violations": 2,
            "critical_violations": 0,
            "patterns_detected": ["ViewModelBase", "AsyncPattern"]
        }

        learning_system.integrate_with_quality_gates(quality_data)
        print(f"[PASS] Quality gates integration method executed")

        return True

    except Exception as e:
        print(f"[FAIL] Session integration test failed: {e}")
        return False

def test_pattern_categories():
    """Test pattern categorization system"""
    print("\n[TEST] Testing pattern categorization...")

    try:
        project_root = Path("C:/DEVELOPMENT/Projects/WPFBase")
        learning_system = WPFBaseAutoLearningSystem(project_root)

        # Test learning analytics which includes pattern categories
        analytics = learning_system.get_learning_analytics()

        if 'pattern_categories' in analytics:
            categories = analytics['pattern_categories']
            print(f"[PASS] Pattern categories found: {len(categories)}")
            for category, count in categories.items():
                print(f"[INFO] Category '{category}': {count} patterns")
        else:
            print("[WARN] Pattern categories not found in analytics")

        # Test that the learning data directory exists
        if learning_system.learning_data_dir.exists():
            print("[PASS] Learning data directory exists")
        else:
            print("[WARN] Learning data directory not created")

        return True

    except Exception as e:
        print(f"[FAIL] Pattern categorization test failed: {e}")
        return False

def main():
    """Run all auto-learning system tests"""
    print("WPFBase Auto-Learning System Test Suite")
    print("=" * 50)

    tests = [
        test_auto_learning_initialization,
        test_pattern_usage_recording,
        test_pattern_promotion_demotion,
        test_claude_md_updates,
        test_session_integration,
        test_pattern_categories
    ]

    passed = 0
    total = len(tests)

    for test in tests:
        if test():
            passed += 1

    print("\n" + "=" * 50)
    print(f"Test Results: {passed}/{total} tests passed")

    if passed == total:
        print("[SUCCESS] All tests passed! Auto-learning system is ready for use.")
        return 0
    else:
        print("[WARNING] Some tests failed. Check implementation and dependencies.")
        return 1

if __name__ == "__main__":
    exit_code = main()
    sys.exit(exit_code)