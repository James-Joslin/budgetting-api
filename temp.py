from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from datetime import date
import psycopg2
from psycopg2.extras import RealDictCursor

app = FastAPI()

# Database connection
conn = psycopg2.connect(
    dbname="your_db",
    user="your_user",
    password="your_pass",
    host="localhost"
)
cur = conn.cursor()

class AccountPayload(BaseModel):
    person_name: str
    account_name: str
    start_date: date
    starting_balance: float

@app.post("/account-with-person")
def create_account_with_person(payload: AccountPayload):
    try:
        # 1. Insert or get person
        cur.execute(
            "INSERT INTO people (name) VALUES (%s) ON CONFLICT (name) DO UPDATE SET name=EXCLUDED.name RETURNING id;",
            (payload.person_name,)
        )
        person_id = cur.fetchone()[0]

        # 2. Insert account
        cur.execute(
            "INSERT INTO accounts (name, owner) VALUES (%s, %s) RETURNING id;",
            (payload.account_name, person_id)
        )
        account_id = cur.fetchone()[0]

        # 3. Insert initial transaction
        cur.execute(
            "INSERT INTO transactions (account_id, amount, date, description) VALUES (%s, %s, %s, %s);",
            (account_id, payload.starting_balance, payload.start_date, "Initial Deposit")
        )

        conn.commit()
        return {"message": "Account setup completed", "account_id": account_id}

    except Exception as e:
        conn.rollback()
        raise HTTPException(status_code=500, detail=str(e))
