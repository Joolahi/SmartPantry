import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'pages/scan_page.dart';
import 'pages/product_list.dart';
import 'pages/settings_page.dart';
import 'package:firebase_core/firebase_core.dart';
import 'services/auth_service.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'pages/login_page.dart';
import 'pages/register_page.dart';
import 'pages/user_page.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await dotenv.load();
  await Firebase.initializeApp();
  await AuthService.singInAnonymouslyIfNeeded();

  final prefs = await SharedPreferences.getInstance();
  final savedToken = prefs.getString('token');
  final savedUserName = prefs.getString('userName');

  runApp(
    MyApp(isLoggedIn: savedToken != null, savedUserName: savedUserName),
  );
}

class MyApp extends StatelessWidget {
  final bool isLoggedIn;
  final String? savedUserName;

  const MyApp({super.key, required this.isLoggedIn, this.savedUserName});
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: "SmartPantry",
      theme: ThemeData(
        useMaterial3: true,
        colorScheme: const ColorScheme(
          brightness: Brightness.light,
          primary: Color(0xFF2E7D32), // Syvä vihreä
          onPrimary: Colors.white,
          secondary: Color(0xFFEF6C00), // Tumma oranssi
          onSecondary: Colors.white,
          surface: Color(0xFFF5F0E6), // Pehmeä beige
          onSurface: Color(0xFF3E3E3E), // Teksti
          error: Colors.red,
          onError: Colors.white,
        ),
        scaffoldBackgroundColor: const Color(0xFFE8F5E9),
        appBarTheme: const AppBarTheme(
          backgroundColor: Color(0xFF2E7D32),
          foregroundColor: Colors.white,
        ),
        textTheme: const TextTheme(
          bodyMedium: TextStyle(color: Color(0xFF3E3E3E)),
        ),
        elevatedButtonTheme: ElevatedButtonThemeData(
          style: ElevatedButton.styleFrom(
            backgroundColor: Color(0xFFEF6C00),
            foregroundColor: Colors.white,
          ),
        ),
        outlinedButtonTheme: OutlinedButtonThemeData(
          style: OutlinedButton.styleFrom(foregroundColor: Color(0xFF2E7D32)),
        ),
      ),
      home: isLoggedIn
          ? MyHomePage(
              title: "SmartPantry",
              userName: savedUserName ?? 'Käyttäjä',
            )
          : const LoginPage(),
      debugShowCheckedModeBanner: false,
       //initialRoute: '/login',
      routes: {
        "/login": (context) => const LoginPage(),
        "/register": (context) => const RegisterPage(),
      },
    );
  }
}

class MyHomePage extends StatefulWidget {
  const MyHomePage({super.key, required this.title, required this.userName});
  final String title;
  final String userName;

  @override
  State<MyHomePage> createState() => _MyHomePageState();
}

class _MyHomePageState extends State<MyHomePage> {
  int _selectedIndex = 0;

  final List<Widget> _pages = [
    ScanPage(),
    ProductListPage(),
    SettingsPage(),
    UserPage(),
  ];

  void _onItemTapped(int index) {
    setState(() {
      _selectedIndex = index;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(widget.userName)),
      body: _pages[_selectedIndex],
      bottomNavigationBar: BottomNavigationBar(
        items: const <BottomNavigationBarItem>[
          BottomNavigationBarItem(
            icon: Icon(Icons.qr_code_scanner),
            label: 'Skannaa',
          ),
          BottomNavigationBarItem(icon: Icon(Icons.list), label: 'Tuotteet'),
          BottomNavigationBarItem(
            icon: Icon(Icons.settings),
            label: 'Asetukset',
          ),
          BottomNavigationBarItem(icon: Icon(Icons.person), label: 'Käyttäjä'),
        ],
        currentIndex: _selectedIndex,
        onTap: _onItemTapped,
        selectedItemColor: Color(0xFF2E7D23),
        unselectedItemColor: Colors.grey[600],
      ),
    );
  }
}
