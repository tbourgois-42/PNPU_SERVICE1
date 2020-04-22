<template>
  <v-layout>
    <v-row>
      <v-col cols="4">
        <v-card class="mx-auto" max-width="500">
          <v-sheet class="pa-4 primary">
            <v-text-field
              append-icon="mdi-magnify"
              v-model="search"
              label="Chercher un élément"
              dark
              flat
              solo-inverted
              hide-details
              clearable
              clear-icon="mdi-close-circle-outline"
            ></v-text-field>
          </v-sheet>
          <v-card-text>
            <v-treeview
              v-model="selection"
              :items="items"
              selection-type="leaf"
              :search="search"
              :filter="filter"
              hoverable
              returnObject
              transition
              activatable
              @update:active="getSelected($event)"
            >
              <template v-slot:prepend="{ item, open }">
                <v-icon v-if="!item.file">
                  {{ open ? 'mdi-folder-open' : 'mdi-folder' }}
                </v-icon>
                <v-icon v-else>
                  mdi-folder
                </v-icon>
              </template>
              <template v-slot:append="{ item }">
                <v-icon
                  v-if="item.file === 'mdi-check-circle'"
                  color="success"
                  >{{ item.file }}</v-icon
                ><v-icon v-else color="error">{{ item.file }}</v-icon>
              </template>
            </v-treeview>
          </v-card-text>
        </v-card>
      </v-col>
      <v-col cols="8">
        <v-list-item-group class="mb-5 d-flex">
          <v-list-item-icon>
            <v-icon>mdi-folder</v-icon>
          </v-list-item-icon>
          <v-list-item-content>
            <v-list-item-title class="subtitle-1 mb-1">
              {{ titleTable }}</v-list-item-title
            >
          </v-list-item-content>
        </v-list-item-group>
        <transition appear name="fade">
          <v-card>
            <v-simple-table>
              <template v-slot:default>
                <thead>
                  <tr>
                    <th class="text-left">Id</th>
                    <th class="text-left">Nom</th>
                    <th class="text-left">Statut</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="item in selectedItemTable" :key="item.name">
                    <td>{{ item.id }}</td>
                    <td>{{ item.name }}</td>
                    <td>
                      <v-icon
                        v-if="item.file === 'mdi-check-circle'"
                        color="success"
                        >{{ item.file }}</v-icon
                      >
                      <v-icon v-else color="error">{{ item.file }}</v-icon>
                    </td>
                  </tr>
                </tbody>
              </template>
            </v-simple-table>
          </v-card>
        </transition>
      </v-col>
    </v-row>
  </v-layout>
</template>

<script>
import Report from '../data/Report.json'
export default {
  data: () => ({
    rapport: Report,
    headers: [
      {
        text: 'Contrôle',
        align: 'start',
        sortable: false,
        value: 'id'
      },
      { text: 'Debut', value: 'debut' },
      { text: 'Fin', value: 'fin' },
      { text: 'Source', value: 'source' },
      { text: 'Id', value: 'id' },
      { text: 'Contrôle', value: 'controle' },
      { text: 'Id', value: 'id' },
      { text: 'Result', value: 'result' },
      { text: 'Message', value: 'message' }
    ],
    processName: '',
    debutExecution: '',
    finExecution: '',
    lstPackageMdb: [],
    lstControleMdb: [],
    search: null,
    caseSensitive: false,
    open: ['public'],
    selection: [],
    items: [
      {
        id: 1,
        name: 'Base de données (.mdb)',
        file: 'mdi-alert-circle',
        children: [
          {
            id: 2,
            name: '02_8.1_HF2003_PLFR_HP.mdb',
            file: 'mdi-alert-circle',
            children: [
              {
                id: 3,
                name: 'Contrôles',
                file: 'mdi-alert-circle',
                children: [
                  {
                    id: 301,
                    name: 'ControleCmdInterdites',
                    file: 'mdi-check-circle'
                  },
                  {
                    id: 302,
                    name: 'ControleIDSynonym',
                    file: 'mdi-check-circle'
                  },
                  {
                    id: 303,
                    name: 'ControleParamAppli',
                    file: 'mdi-check-circle'
                  },
                  {
                    id: 304,
                    name: 'ControleTacheSecu',
                    file: 'mdi-alert-circle',
                    children: [
                      {
                        id: 3041,
                        name: 'Tâche SFR_RECAP_ANN_PAS non sécurisée.'
                      }
                    ]
                  },
                  {
                    id: 305,
                    name: 'ControleTableSecu',
                    file: 'mdi-alert-circle',
                    children: [
                      {
                        id: 3051,
                        name: 'Table SFR_SEPA_XML_NOMMAGE non sécurisée.'
                      }
                    ]
                  },
                  {
                    id: 306,
                    name: 'ControleTypePack',
                    file: 'mdi-alert-circle',
                    children: [
                      {
                        id: 3061,
                        name:
                          'La commande 1 du pack 8.1_HF2003_SFR_143626_L est interdite.'
                      },
                      {
                        id: 3062,
                        name:
                          'La commande 2 du pack 8.1_HF2003_SFR_143626_L est interdite.'
                      },
                      {
                        id: 3063,
                        name:
                          'La commande 3 du pack 8.1_HF2003_SFR_143626_L est interdite.'
                      },
                      {
                        id: 3064,
                        name:
                          'La commande 26 du pack 8.1_HF2003_SFR_143626_L est interdite.'
                      },
                      {
                        id: 3065,
                        name:
                          'La commande 3 du pack 8.1_HF2003_SFR_139306_D est interdite.'
                      },
                      {
                        id: 3066,
                        name:
                          'La commande 1 du pack 8.1_HF2003_SFR_139306_F est interdite.'
                      }
                    ]
                  }
                ]
              }
            ]
          },
          {
            id: 4,
            name: '8.1_HF2003_PLFR_PAY.mdb',
            file: 'mdi-alert-circle',
            children: [
              {
                id: 41,
                name: 'Contrôles',
                file: 'mdi-alert-circle',
                children: [
                  {
                    id: 411,
                    name: 'ControleCmdInterdites',
                    file: 'mdi-alert-circle',
                    children: [
                      {
                        id: 4111,
                        name:
                          'La commande 3 du pack 8.1_HF2003_PLFR_123079_L est interdite dans D:\\PNPU\\8.1_HF2003_PLFR_PAY.mdb'
                      }
                    ]
                  },
                  {
                    id: 412,
                    name: 'ControleIDSynonym',
                    file: 'mdi-check-circle'
                  },
                  {
                    id: 413,
                    name: 'ControleParamAppli',
                    file: 'mdi-check-circle'
                  },
                  {
                    id: 414,
                    name: 'ControleTacheSecu',
                    file: 'mdi-check-circle'
                  },
                  {
                    id: 415,
                    name: 'ControleTableSecu',
                    file: 'mdi-check-circle'
                  },
                  {
                    id: 416,
                    name: 'ControleTypePack',
                    file: 'mdi-alert-circle',
                    children: [
                      {
                        id: 4161,
                        name:
                          'La commande 2 du pack 8.1_HF2003_PLFR_148843_D est interdite.'
                      },
                      {
                        id: 4162,
                        name:
                          'La commande 2 du pack 8.1_HF2003_SFR_145375_D est interdite.'
                      },
                      {
                        id: 4163,
                        name:
                          'La commande 2 du pack 8.1_HF2003_PLFR_137306_D est interdite.'
                      },
                      {
                        id: 4164,
                        name:
                          'La commande 2 du pack 8.1_HF2003_PLFR_74326_D est interdite.'
                      },
                      {
                        id: 4165,
                        name:
                          'La commande 1 du pack 8.1_HF2003_PLFR_137306_F est interdite.'
                      },
                      {
                        id: 4166,
                        name:
                          'La commande 1 du pack 8.1_HF2003_PLFR_142091_F est interdite.'
                      },
                      {
                        id: 4167,
                        name:
                          'La commande 1 du pack 8.1_HF2003_PLFR_74326_F est interdite.'
                      }
                    ]
                  }
                ]
              }
            ]
          }
        ]
      }
    ],
    active: [],
    selectedItemTable: [],
    titleTable: ''
  }),

  computed: {
    filter() {
      return this.caseSensitive
        ? (item, search, textKey) => item[textKey].includes(search) > -1
        : undefined
    }
  },

  created() {
    /* console.log('this.item', this.rapport)
    let str = JSON.stringify(this.rapport)
    str = str.replace(/source/g, 'Children')
    str = str.replace(/controle/g, 'Children')
    str = str.replace(/message/g, 'Children')

    // object = JSON.parse(str)
    console.log('str', str)
    this.items = JSON.parse(str)
    console.log('this.items', this.items) */
    this.selectedItemTable = this.items[0].children
    this.titleTable = this.items[0].name
  },

  methods: {
    getSelected(e) {
      if (e.length > 0) {
        if (e[0].children !== undefined) {
          for (const selectedItem of e) {
            this.selectedItemTable = selectedItem.children
            this.titleTable = selectedItem.name
          }
        }
      }
    }
  }
}
</script>

<style lang="scss" scoped>
.v-treeview-node__content {
  cursor: pointer !important;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.5s;
}
.fade-enter, .fade-leave-to /* .fade-leave-active below version 2.1.8 */ {
  opacity: 0;
}
</style>
