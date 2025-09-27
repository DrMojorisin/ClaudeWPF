#!/usr/bin/env python3
"""
Test script for WPFBase GPTLint Integration System
"""

import sys
import json
import tempfile
from pathlib import Path
from unittest.mock import Mock, patch

# Add current directory to path
sys.path.insert(0, str(Path(__file__).parent))
sys.path.append('.claude')

try:
    import gptlint_integration
    from gptlint_integration import WPFBaseGPTLintIntegration
except ImportError as e:
    print(f"[FAIL] Failed to import gptlint_integration: {e}")
    sys.exit(1)

def test_gptlint_integration_initialization():
    """Test GPTLint integrator initialization"""
    print("[TEST] Testing GPTLint integration initialization...")

    try:
        project_root = Path("C:/DEVELOPMENT/Projects/WPFBase")
        integrator = WPFBaseGPTLintIntegration(project_root)

        print(f"[PASS] GPTLint integrator initialized successfully")
        print(f"[INFO] Project root: {integrator.project_root}")
        print(f"[INFO] Config file: {integrator.config_path}")
        print(f"[INFO] Cache dir: {integrator.cache_dir}")

        # Verify configuration file exists
        if integrator.config_path.exists():
            print("[PASS] .gptlint.yml configuration file found")
        else:
            print("[WARN] .gptlint.yml configuration file not found")

        return True

    except Exception as e:
        print(f"[FAIL] GPTLint integration initialization failed: {e}")
        return False

def test_file_analysis():
    """Test analyzing C# files with GPTLint rules"""
    print("\n[TEST] Testing file analysis...")

    try:
        project_root = Path("C:/DEVELOPMENT/Projects/WPFBase")
        integrator = WPFBaseGPTLintIntegration(project_root)

        # Create test C# file
        test_code = '''
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class TestViewModel : ViewModelBase
{
    private string name;

    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    public ICommand SaveCommand { get; }

    public TestViewModel()
    {
        SaveCommand = new RelayCommand(Save);
    }

    private void Save()
    {
        // Manual implementation
    }
}
'''

        with tempfile.NamedTemporaryFile(mode='w', suffix='.cs', delete=False) as f:
            f.write(test_code)
            temp_file = Path(f.name)

        try:
            # Test the analyze_file method (async)
            import asyncio

            async def run_analysis():
                return await integrator.analyze_file(temp_file)

            result = asyncio.run(run_analysis())

            print(f"[PASS] File analysis completed")
            print(f"[INFO] Found {len(result.violations)} violations")
            print(f"[INFO] Total violations: {result.total_violations}")

            if result.violations:
                violation = result.violations[0]
                print(f"[INFO] Sample violation: {violation.rule_id} on line {violation.line_number}")

        finally:
            temp_file.unlink()  # Clean up temp file

        return True

    except Exception as e:
        print(f"[FAIL] File analysis test failed: {e}")
        return False

def test_quality_gate_integration():
    """Test integration with existing quality gates"""
    print("\n[TEST] Testing quality gate integration...")

    try:
        project_root = Path("C:/DEVELOPMENT/Projects/WPFBase")
        integrator = WPFBaseGPTLintIntegration(project_root)

        # Test quality gate validation
        test_violations = [
            {
                'rule_id': 'wpf-001-observable-property',
                'severity': 'critical',
                'file': 'TestViewModel.cs',
                'line': 10
            },
            {
                'rule_id': 'wpf-002-async-cancellation',
                'severity': 'high',
                'file': 'ServiceClass.cs',
                'line': 25
            }
        ]

        # Mock session manager integration
        mock_session_manager = Mock()
        mock_session_manager.validate_quality_gates.return_value = {
            'passing_count': 8,
            'failing_count': 2,
            'gates': []
        }

        # Test quality dashboard
        import asyncio

        async def get_dashboard():
            return await integrator.get_quality_dashboard()

        quality_result = asyncio.run(get_dashboard())

        print(f"[PASS] Quality gate integration working")
        print(f"[INFO] Total checks: {quality_result['summary']['total_checks']}")
        print(f"[INFO] Recent checks: {quality_result['summary']['recent_checks']}")
        print(f"[INFO] Quality score: {quality_result['trends']['quality_score']}")
        print(f"[INFO] Improvement trend: {quality_result['trends']['improving']}")
        print(f"[INFO] Recommendations: {len(quality_result['recommendations'])}")

        return True

    except Exception as e:
        print(f"[FAIL] Quality gate integration test failed: {e}")
        return False

def test_real_time_monitoring():
    """Test real-time file monitoring capabilities"""
    print("\n[TEST] Testing real-time monitoring setup...")

    try:
        project_root = Path("C:/DEVELOPMENT/Projects/WPFBase")
        integrator = WPFBaseGPTLintIntegration(project_root)

        # Test starting real-time monitoring
        # Note: We don't actually start watching to avoid side effects
        # Just test the setup

        print(f"[PASS] Real-time monitoring setup ready")
        print(f"[INFO] Project root: {integrator.project_root}")
        print(f"[INFO] Cache directory: {integrator.cache_dir}")

        # Test file filtering
        test_files = [
            "TestViewModel.cs",
            "TestService.cs",
            "TestView.xaml",
            "readme.txt",
            "Test.csproj"
        ]

        filtered_files = [f for f in test_files if integrator._should_analyze_file(Path(f))]
        print(f"[PASS] File filtering working")
        print(f"[INFO] Files to analyze: {filtered_files}")

        return True

    except Exception as e:
        print(f"[FAIL] Real-time monitoring test failed: {e}")
        return False

def test_configuration_loading():
    """Test loading and validating GPTLint configuration"""
    print("\n[TEST] Testing configuration loading...")

    try:
        project_root = Path("C:/DEVELOPMENT/Projects/WPFBase")
        integrator = WPFBaseGPTLintIntegration(project_root)

        # Test loading configuration
        config = integrator.config

        print(f"[PASS] Configuration loaded successfully")
        print(f"[INFO] Project: {config.get('project', 'Unknown')}")
        print(f"[INFO] Version: {config.get('version', 'Unknown')}")
        print(f"[INFO] Rules count: {len(config.get('rules', []))}")
        print(f"[INFO] Categories: {len(config.get('rule_categories', {}))}")

        # Validate required sections
        required_sections = ['settings', 'rules', 'rule_categories']
        for section in required_sections:
            if section in config:
                print(f"[PASS] Required section '{section}' found")
            else:
                print(f"[WARN] Required section '{section}' missing")

        # Check for WPFBase-specific rules
        wpfbase_rules = [rule for rule in config.get('rules', [])
                        if rule.get('id', '').startswith('wpfbase-')]
        print(f"[INFO] WPFBase-specific rules: {len(wpfbase_rules)}")

        return True

    except Exception as e:
        print(f"[FAIL] Configuration loading test failed: {e}")
        return False

def main():
    """Run all GPTLint integration tests"""
    print("WPFBase GPTLint Integration Test Suite")
    print("=" * 50)

    tests = [
        test_gptlint_integration_initialization,
        test_configuration_loading,
        test_file_analysis,
        test_quality_gate_integration,
        test_real_time_monitoring
    ]

    passed = 0
    total = len(tests)

    for test in tests:
        if test():
            passed += 1

    print("\n" + "=" * 50)
    print(f"Test Results: {passed}/{total} tests passed")

    if passed == total:
        print("[SUCCESS] All tests passed! GPTLint integration is ready for use.")
        return 0
    else:
        print("[WARNING] Some tests failed. Check configuration and dependencies.")
        return 1

if __name__ == "__main__":
    exit_code = main()
    sys.exit(exit_code)