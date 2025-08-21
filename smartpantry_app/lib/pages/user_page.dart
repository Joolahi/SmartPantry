// settings_page.dart
import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../pages/login_page.dart';

class UserPage extends StatelessWidget {
  const UserPage({super.key});

  Future<void> _logout(BuildContext context) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove('token');
    await prefs.remove('userName');

    Navigator.pushAndRemoveUntil(
      context,
      MaterialPageRoute(builder: (context) => const LoginPage()),
      (route) => false,
    );
  }

  @override
  Widget build(BuildContext context) {
    return Center(
      child: ElevatedButton.icon(
        icon: const Icon(Icons.logout),
        label: const Text("Kirjaudu ulos"),
        onPressed: () => _logout(context),
        style: ElevatedButton.styleFrom(
          backgroundColor: const Color(0xFFEF6C00),
          foregroundColor: Colors.white,
        ),
      ),
    );
  }
}
