import 'package:flutter/material.dart';
import 'package:barcode_scan2/barcode_scan2.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';
import 'product_details_page.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';

class ScanPage extends StatefulWidget {
  const ScanPage({super.key});
  @override
  ScanPageState createState() => ScanPageState();
}

class ScanPageState extends State<ScanPage> {
  bool isLoading = false;

  Future<void> scanBarcode() async {
    var result = await BarcodeScanner.scan();
    var code = result.rawContent;
    if (code.isEmpty) return;
    final String baseUrl = dotenv.env['BASE_URL1'] ?? '';
    var url = '$baseUrl/barcode/$code';

    setState(() {
      isLoading = true;
    });

    try {
      final response = await http.get(Uri.parse(url));
      if (!mounted) return; // Check if the widget is still mounte
      if (response.statusCode == 200) {
        var data = json.decode(response.body);

        final result = await Navigator.push(
          context,
          MaterialPageRoute(
            builder: (_) => ProductDetailsPage(
              barcode: code,
              name: data['name'] ?? 'N/A',
              brand: data['brand'] ?? 'N/A',
              energy: data['energy'] != null
                  ? double.parse(data['energy'].toString())
                  : 0.0,
              imageThumbUrl: data['image'] ?? '',
            ),
          ),
        );

        if (result == true) {
          await scanBarcode(); // Käynnistä uusi skannaus jos lisättiin
        }
      } else {
        _showErrorDialog('Tuotetta ei löydy');
      }
    } catch (e) {
      _showErrorDialog('Virhe haettaessa tuotetta');
    } finally {
      setState(() {
        isLoading = false;
      });
    }
  }

  void _showErrorDialog(String message) {
    showDialog(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Virhe'),
        content: Text(message),
        actions: [
          TextButton(
            child: const Text('OK'),
            onPressed: () => Navigator.pop(context),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Center(
      child: isLoading
          ? const CircularProgressIndicator()
          : ElevatedButton(
              onPressed: scanBarcode,
              child: const Text('Skannaa viivakoodi'),
            ),
    );
  }
}
