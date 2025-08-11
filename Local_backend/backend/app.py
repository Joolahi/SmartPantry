from fastapi import FastAPI
import os 
import psycopg2

app = FastAPI()

@app.get("/")
def read_root():
    return {"message": "Welcome to the Smart Pantry API"}


@app.get("/test-db")
def test_db():
    try:
        conn = psycopg2.connect(os.environ["DATABASE_URL"])
        cur = conn.cursor()
        cur.execute("SELECT version();")
        db_version = cur.fetchone()
        cur.close()
        conn.close()
        return {"db_version": db_version}
    except Exception as e:
        return {"error": str(e)}
