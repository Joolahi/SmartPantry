import 'package:flutter/material.dart';
//import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:smartpantry_app/services/auth_service.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';

class ProductDetailsPage extends StatefulWidget {
  final String barcode;
  final String name;
  final String brand;
  final double energy;
  final String? imageThumbUrl;

  const ProductDetailsPage({
    super.key,
    required this.barcode,
    required this.name,
    required this.brand,
    required this.energy,
    this.imageThumbUrl,
  });

  @override
  State<ProductDetailsPage> createState() => _ProductDetailsPageState();
}

class _ProductDetailsPageState extends State<ProductDetailsPage> {
  DateTime? _bestBefore;

  void _selectDate(BuildContext context) async {
    final picked = await showDatePicker(
      context: context,
      initialDate: DateTime.now(),
      firstDate: DateTime.now().subtract(const Duration(days: 1)),
      lastDate: DateTime(2100),
    );

    if (picked != null) {
      setState(() {
        _bestBefore = picked;
      });
    }
  }

  void _addProductAndReturn() async {
    final uid = AuthService.currentUser?.uid;
    if (uid == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Käyttäjä ei ole kirjautunut sisään")),
      );
      return;
    }

    if (_bestBefore == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Valitse parasta ennen -päiväys")),
      );
      return;
    }

    final response = await http.post(
      Uri.parse('http://192.168.100.11:8000/barcode'),
      headers: {'Content-Type': 'application/json'},
      body: json.encode({
        'userId': uid,
        'barcode': widget.barcode,
        'name': widget.name,
        'brand': widget.brand,
        'energy': widget.energy,
        'bestBefore': _bestBefore!.toIso8601String(),
        'image': widget.imageThumbUrl ?? '',
      }),
    );
    /*
    await FirebaseFirestore.instance
        .collection('users')
        .doc(uid)
        .collection('products')
        .add({
          'barcode': widget.barcode,
          'name': widget.name,
          'brand': widget.brand,
          'energy': widget.energy,
          'bestBefore': _bestBefore!.toIso8601String(),
          'createdAt': FieldValue.serverTimestamp(),
          'imageThumbUrl': widget.imageThumbUrl ?? '',
        });
  */
    if (response.statusCode == 200) {
      Navigator.pop(context, true); // Retrurn true to indicate success
      return;
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Tuotteen lisääminen epäonnistui!')),
      );
      return;
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text("Tuotetiedot")),
      body: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          children: [
            Text("Tuote: ${widget.name}", style: const TextStyle(fontSize: 18)),
            Text("Brändi: ${widget.brand}"),
            Text("Energia: ${widget.energy.toStringAsFixed(0)} kcal"),
            const SizedBox(height: 16),
            Row(
              children: [
                Text(
                  _bestBefore == null
                      ? "Parasta ennen: ei valittu"
                      : "Parasta ennen: ${_bestBefore!.toLocal().toString().split(' ')[0]}",
                ),
                const SizedBox(width: 10),
                ElevatedButton(
                  onPressed: () => _selectDate(context),
                  child: const Text("Valitse päiväys"),
                ),
              ],
            ),
            const Spacer(),
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceEvenly,
              children: [
                OutlinedButton(
                  onPressed: () => Navigator.pop(context, false),
                  child: const Text("Peruuta"),
                ),
                ElevatedButton(
                  onPressed: _addProductAndReturn,
                  child: const Text("Lisää tietokantaan"),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
