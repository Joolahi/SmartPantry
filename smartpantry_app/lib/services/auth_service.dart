import 'package:firebase_auth/firebase_auth.dart';

class AuthService {
  static final FirebaseAuth _auth = FirebaseAuth.instance;

  static Future<User?> singInAnonymouslyIfNeeded() async {
    User? user = _auth.currentUser;
    if (user == null) {
      final result = await _auth.signInAnonymously();
      return result.user;
    }
    return user;
  }

  static User? get currentUser => _auth.currentUser;
}
