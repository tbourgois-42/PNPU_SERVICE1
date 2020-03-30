<template>
  <v-layout>
    <v-container class="grey lighten-5">
      <v-list-item>
        <v-list-item-content>
          <v-list-item-title class="title">
            Paramétrage des workflows
          </v-list-item-title>
          <v-list-item-subtitle>
            subtext
          </v-list-item-subtitle>
        </v-list-item-content>
      </v-list-item>
      <v-divider class="my-2 mx-4" inset></v-divider>
      <v-row>
        <v-col cols="12" sm="6">
          <v-data-table :headers="headers"
                        :items="workflows"
                        sort-by="calories"
                        class="elevation-1">
            <template v-slot:top>
              <v-toolbar flat color="white">
                <v-toolbar-title>Mes workflows</v-toolbar-title>
                <v-divider class="mx-4" inset vertical></v-divider>
                <v-spacer></v-spacer>
                <v-dialog v-model="dialog" max-width="500px">
                  <template v-slot:activator="{ on }">
                    <v-btn color="primary" dark class="mb-2" v-on="on">Créer un workflow</v-btn>
                  </template>
                  <v-card>
                    <v-card-title>
                      <span class="headline">{{ formTitle }}</span>
                    </v-card-title>

                    <v-card-text>
                      <v-container>
                        <v-row>
                          <v-col cols="12" sm="6" md="12">
                            <v-text-field v-model="editedItem.name"
                                          label="Nom du processus"></v-text-field>
                          </v-col>
                          <v-col cols="12" sm="6" md="4">
                            <v-select :items="editedItem.calories"
                                      label="Ordre"
                                      solo></v-select>
                          </v-col>
                        </v-row>
                      </v-container>
                    </v-card-text>

                    <v-card-actions>
                      <v-spacer></v-spacer>
                      <v-btn color="blue darken-1" text @click="close">Cancel</v-btn>
                      <v-btn color="blue darken-1" text @click="save">Save</v-btn>
                    </v-card-actions>
                  </v-card>
                </v-dialog>
              </v-toolbar>
            </template>
            <template v-slot:item.actions="{ item }">
              <v-icon small class="mr-2" @click="editItem(item)">
                mdi-pencil
              </v-icon>
              <v-icon small @click="deleteItem(item)">
                mdi-delete
              </v-icon>
            </template>
            <template v-slot:no-data>
              <v-btn color="primary" @click="initialize">Reset</v-btn>
            </template>
          </v-data-table>
        </v-col>
        <v-col cols="12" sm="6">
          <v-data-table :headers="headers"
                        :items="processus"
                        sort-by="calories"
                        class="elevation-1">
            <template v-slot:top>
              <v-toolbar flat color="white">
                <v-toolbar-title>Mes processus</v-toolbar-title>
                <v-divider class="mx-4" inset vertical></v-divider>
                <v-spacer></v-spacer>
                <v-dialog v-model="dialog" max-width="500px">
                  <template v-slot:activator="{ on }">
                    <v-btn color="primary" dark class="mb-2" v-on="on">Créer un processus</v-btn>
                  </template>
                  <v-card>
                    <v-card-title>
                      <span class="headline">{{ formTitle }}</span>
                    </v-card-title>

                    <v-card-text>
                      <v-container>
                        <v-row>
                          <v-col cols="12" sm="6" md="12">
                            <v-text-field v-model="editedItem.name"
                                          label="Nom du processus"></v-text-field>
                          </v-col>
                          <v-col cols="12" sm="6" md="4">
                            <v-select :items="editedItem.calories"
                                      label="Ordre"
                                      solo></v-select>
                          </v-col>
                        </v-row>
                      </v-container>
                    </v-card-text>

                    <v-card-actions>
                      <v-spacer></v-spacer>
                      <v-btn color="blue darken-1" text @click="close">Cancel</v-btn>
                      <v-btn color="blue darken-1" text @click="save">Save</v-btn>
                    </v-card-actions>
                  </v-card>
                </v-dialog>
              </v-toolbar>
            </template>
            <template v-slot:item.actions="{ item }">
              <v-icon small class="mr-2" @click="editItem(item)">
                mdi-pencil
              </v-icon>
              <v-icon small @click="deleteItem(item)">
                mdi-delete
              </v-icon>
            </template>
            <template v-slot:no-data>
              <v-btn color="primary" @click="initialize">Reset</v-btn>
            </template>
          </v-data-table>
        </v-col>
      </v-row>
    </v-container>
  </v-layout>
</template>

<script>
  export default {
    data: () => ({
      dialog: false,
      headers: [
        {
          text: 'Nom',
          align: 'start',
          sortable: false,
          value: 'name'
        },
        { text: 'Ordre', value: 'calories' },
        { text: 'Actions', value: 'actions', sortable: false }
      ],
      workflows: [],
      processus: [],
      editedIndex: -1,
      editedItem: {
        name: '',
        calories: 0,
        fat: 0,
        carbs: 0,
        protein: 0
      },
      defaultItem: {
        name: '',
        calories: 0,
        fat: 0,
        carbs: 0,
        protein: 0
      }
    }),

    computed: {
      formTitle() {
        return this.editedIndex === -1
          ? "Ajout d'un processus"
          : "Edition d'un processus"
      }
    },

    watch: {
      dialog(val) {
        val || this.close()
      }
    },

    created() {
      this.initialize()
    },

    methods: {
      initialize() {
        this.processus = [
          {
            name: 'Pré-Contrôle du HF',
            calories: '01',
            fat: 6.0,
            carbs: 24,
            protein: 4.0
          },
          {
            name: 'Initialisation',
            calories: '02',
            fat: 9.0,
            carbs: 37,
            protein: 4.3
          },
          {
            name: 'Packaging des dépendances',
            calories: '03',
            fat: 16.0,
            carbs: 23,
            protein: 6.0
          },
          {
            name: "Analyse d'impact",
            calories: '04',
            fat: 3.7,
            carbs: 67,
            protein: 4.3
          },
          {
            name: "Tests d'intégration",
            calories: '05',
            fat: 16.0,
            carbs: 49,
            protein: 3.9
          },
          {
            name: 'Tests des processus critiques',
            calories: '06',
            fat: 0.0,
            carbs: 94,
            protein: 0.0
          },
          {
            name: 'TNR Standard',
            calories: '07',
            fat: 0.2,
            carbs: 98,
            protein: 0
          },
          {
            name: 'Livraison',
            calories: '08',
            fat: 3.2,
            carbs: 87,
            protein: 6.5
          }
        ]
      },

      editItem(item) {
        this.editedIndex = this.processus.indexOf(item)
        this.editedItem = Object.assign({}, item)
        this.dialog = true
      },

      deleteItem(item) {
        const index = this.processus.indexOf(item)
        confirm('Are you sure you want to delete this item?') &&
          this.processus.splice(index, 1)
      },

      close() {
        this.dialog = false
        setTimeout(() => {
          this.editedItem = Object.assign({}, this.defaultItem)
          this.editedIndex = -1
        }, 300)
      },

      save() {
        if (this.editedIndex > -1) {
          Object.assign(this.processus[this.editedIndex], this.editedItem)
        } else {
          this.processus.push(this.editedItem)
        }
        this.close()
      }
    }
  }
</script>
