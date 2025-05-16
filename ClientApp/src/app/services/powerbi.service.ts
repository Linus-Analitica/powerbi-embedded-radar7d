import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';

// Interfaz para la información de embebido (ajustar según tu backend)
export interface EmbedInfo {
  accessToken: string;
  embedUrl: string;
  expiry: string;
  reportId?: string; // Opcional, depende de tu implementación
  datasetId?: string; // Opcional
}

@Injectable({
  providedIn: 'root' // Proveer el servicio globalmente
})
export class PowerBiService {

  constructor(private http: HttpClient) { }

  getEmbedInfo(): Observable<EmbedInfo> {
    console.warn('PowerBiService: getEmbedInfo() está devolviendo datos HARCODEADOS para pruebas.');

    // --- INICIO: Hardcoding para pruebas ---
    const hardcodedEmbedUrl = 'https://app.powerbi.com/reportEmbed?reportId=38ae0459-3488-436d-bdfc-ec209e61d3fa&groupId=7d59785b-b611-4e91-9d03-c821b7ad002e&w=2&config=eyJjbHVzdGVyVXJsIjoiaHR0cHM6Ly9XQUJJLVBBQVMtMS1TQ1VTLXJlZGlyZWN0LmFuYWx5c2lzLndpbmRvd3MubmV0IiwiZW1iZWRGZWF0dXJlcyI6eyJ1c2FnZU1ldHJpY3NWTmV4dCI6dHJ1ZX19';
    const hardcodedAccessToken = 'H4sIAAAAAAAEAB2Ux6q0aABE3-VuHTC0ceBftLHNOe7M-TNr6zDvPpfZFgcKDlT982Ol9zClxc_fP56sIRWX4z46jguMqn34aGVPwkGPlTPlhkRwDmUqtsuV84ajjcC3lhEXSdTscaOlb3rHWSJ20XPVthhFCU1y0fHt3OMOQ1tUal7hL_qoik5o-CfA7Zu-hNsYUC1Xpc_-7A3pbxV0L2lHjwEihoD_LGskyMJs3RYN29MwtaO-vuUyBc_Z1I2wPtN4Eqe3cQ1hxNfUn2apEHxj5hw1M2G9BM4635lua4v_nH3iffPqgsWV3BL_fXhmFYFH5-B5giLKvu9K5sEQI4ffBXdfVrBaCkVGjJ8k7hVZPy9Djyw8VOawVnW7QyE5xmUkFs7kVSnwagBeaFctlfCp5mHm6zBjSGxqxBwMY7MGVdcGggKR1Vrk2Gas6Z4BtGIbOoQ7qpX1uAnHrhAKWYwBiCay5QKyFJaa8Ayw7DuwMgdbetDie_-bz-1uBMJ5fDfkfqcJFcpypftT5njNx1u85Rqj15O3AzO8TF3mQyTZlJWm6GJVvYjGUoWHxR1prjp9sxxaRoP-nBRaMQVX9t4BzvA7tC8LSBtmLy_EsTUaWeuxMg15o1E8xnaFKqJSkCjUHCGZiMXqiyq8gBABO4b4CBvL5IWBzOp4d8DCqEfS97snEmOmOTgWpDTJmUd87LLx_Ha0uRwyvC_ZB6PCQkMxeNk_iRjcY3xuz2-rUhbQXcWDn02H2GG-D-iiWbYdeCSP2OAEIXfLDLqWKWMdjeyJ_cRVlJ5Bad_bAnJzOsO2xRJjUa1bsh6gWM4i2QjGUczSBSPxoq_0HV3ux7hOOjyNhs9bYIHgta7byfhCg3-sGpwPuVpHPxD-cp3JKJ3BaS6vRVUb7zii14Ynx4eUijE5usYnZmh6p5StSC1M2EpzMCQroGYLM0VUtBve5EffNXGdHS05J7jQ6ikELwROxs4HxjkxN5reVa5XozaGZ7bKNb7apCrciv80_Z-fv3649Z73SS3v3-nq8FEJVzVNIKaZpygdq3k7Y7ov0AfLa-GJ-wYKu5zp-cRPHY6lOUdZ46kK2MzigFRlPNy2U4eHW2ap-U7bk9z71SKhA9oW--o-mojurf3FujTyfRyLnz52e0VthabdJnPP39CNnV8ESuMQhLtu0VaUK4mbgEl1qGd-ZUAjohWQDHdhj5dzLhk3CVu-kmB4qu-VFOOHIUL69Y6Z1Gu5vTrCCq4C5laU3cu-LL_zxbPK-8WaiFwyd8W4wn5vc5p77FlLPtVMAA4KNjIrdYMgCSPSB-J8K8jE4JqcocNm9x184wQjZ7RwDgqVkluanJewnev2miWmsGceA40t1PIUWXwIufWf_zXfc1OucvBrWQ7t5NCM9Xzmhj6RnNztXbX_p9y2Bul-rOUv1istVECfAku6TXhWrWl2Z9s-dVJok9h_DvGg0j07u_Cr33GegIJcv9fmmCwlYVF5iBmjM6fZcYmkpbOOyMVII9k9DB7Io3IyhlkJv0lDi5fdDKhEmJj6e2rBOtq_bY_nMFKJyPOs88I0ZfXgrX3A8ueSyDhqhVA2ZkhJoEEe7W3Ork-3lr7qyhaONBU53RZRqcwTQvqYXJyhQHovH_JwRVdAadiOye5ZBC7r4b3LA0hatwT5QmkBoaG1zloIveNqmQKEzje55mCxLhxVZ9uH4rVnDJGC7xjXfo_nFYYphdSTscdAdh1mFdW8yB7snsksv1Foqi1E6NtdHiWSe8zPWh_vX83__gcu84gtrgYAAA==.eyJjbHVzdGVyVXJsIjoiaHR0cHM6Ly9XQUJJLVBBQVMtMS1TQ1VTLXJlZGlyZWN0LmFuYWx5c2lzLndpbmRvd3MubmV0IiwiZXhwIjoxNzQ2MDM5ODUzLCJhbGxvd0FjY2Vzc092ZXJQdWJsaWNJbnRlcm5ldCI6dHJ1ZX0=';

    return of({
      accessToken: hardcodedAccessToken,
      embedUrl: hardcodedEmbedUrl,
      expiry: new Date(Date.now() + 3600 * 1000).toISOString() // Expiración simulada en 1 hora
    });

    // --- FIN: Hardcoding para pruebas ---

  }

}