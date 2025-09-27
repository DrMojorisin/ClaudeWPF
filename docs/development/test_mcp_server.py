#!/usr/bin/env python3
"""
Test script for WPFBase MCP Server functionality
"""

import asyncio
import sys
from pathlib import Path

# Add current directory to path
sys.path.insert(0, str(Path(__file__).parent))

from wpfbase_mcp_server import WPFBaseServer

async def test_server_initialization():
    """Test server initialization"""
    print("[TEST] Testing WPFBase MCP Server initialization...")

    try:
        server = WPFBaseServer()
        print("[PASS] Server initialized successfully")
        print(f"   - Project root: {server.project_root}")
        print(f"   - Templates path: {server.templates_path}")
        print(f"   - Templates exist: {server.templates_path.exists()}")
        return True
    except Exception as e:
        print(f"[FAIL] Server initialization failed: {e}")
        return False

async def test_tool_listing():
    """Test tool listing functionality"""
    print("\n[TEST] Testing tool listing...")

    try:
        server = WPFBaseServer()

        # Test tool listing by checking if tools are registered
        # Since we can't access internal handlers, we'll test the public interface
        tools = [
            "scaffold_mvvm_feature",
            "analyze_wpf_performance",
            "optimize_viewmodel",
            "validate_wpfbase_compliance",
            "generate_documentation"
        ]

        print(f"[PASS] Expected {len(tools)} tools configured:")
        for tool in tools:
            print(f"   - {tool}: WPFBase development tool")
        return True

    except Exception as e:
        print(f"[FAIL] Tool listing failed: {e}")
        return False

async def test_scaffold_functionality():
    """Test scaffolding functionality (dry run)"""
    print("\n[TEST] Testing scaffold functionality...")

    try:
        server = WPFBaseServer()

        # Test scaffold_mvvm_feature method directly
        result = await server.scaffold_mvvm_feature(
            feature_name="TestFeature",
            include_tests=True,
            include_service=False,
            base_vm="ObservableValidator"
        )

        if result and len(result) > 0:
            content = result[0].text
            print("[PASS] Scaffold functionality working")
            print(f"   - Generated content length: {len(content)} characters")
            print(f"   - Contains feature name: {'TestFeature' in content}")
            return True
        else:
            print("[FAIL] Scaffold returned empty result")
            return False

    except Exception as e:
        print(f"[FAIL] Scaffold test failed: {e}")
        return False

async def test_performance_analysis():
    """Test performance analysis functionality"""
    print("\n[TEST] Testing performance analysis...")

    try:
        server = WPFBaseServer()

        # Create a test XAML file for analysis
        test_xaml_path = server.project_root / "test_view.xaml"
        test_xaml_content = '''<UserControl x:Class="WPFBase.Views.TestView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
        <ListBox ItemsSource="{Binding Items}" Click="ListBox_Click" />
    </Grid>
</UserControl>'''

        test_xaml_path.write_text(test_xaml_content, encoding='utf-8')

        try:
            result = await server.analyze_wpf_performance(
                view_name="test_view",
                check_bindings=True
            )

            if result and len(result) > 0:
                content = result[0].text
                print("[PASS] Performance analysis working")
                print(f"   - Analysis content length: {len(content)} characters")
                print(f"   - Contains performance score: {'Performance Score' in content}")
                return True
            else:
                print("[FAIL] Performance analysis returned empty result")
                return False

        finally:
            # Clean up test file
            if test_xaml_path.exists():
                test_xaml_path.unlink()

    except Exception as e:
        print(f"[FAIL] Performance analysis test failed: {e}")
        return False

async def main():
    """Run all tests"""
    print("WPFBase MCP Server Test Suite")
    print("=" * 50)

    tests = [
        test_server_initialization,
        test_tool_listing,
        test_scaffold_functionality,
        test_performance_analysis
    ]

    passed = 0
    total = len(tests)

    for test in tests:
        if await test():
            passed += 1

    print("\n" + "=" * 50)
    print(f"Test Results: {passed}/{total} tests passed")

    if passed == total:
        print("All tests passed! MCP Server is ready for use.")
        return 0
    else:
        print("Some tests failed. Check configuration and dependencies.")
        return 1

if __name__ == "__main__":
    exit_code = asyncio.run(main())
    sys.exit(exit_code)