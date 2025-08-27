from pydantic import BaseModel, EmailStr

class RegisterRequest(BaseModel):
    email: EmailStr
    password: str

class LoginRequest(BaseModel):
    email: EmailStr
    password: str

class BarcodeData(BaseModel):
    barcode: str
    name: str
    brand: str
    energy: float
    image: str
    bestBefore: str = None
    userId: str = None